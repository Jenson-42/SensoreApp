using System.Text;

namespace SensoreApp.Services
{
    public class CSVParserService
    {
        private readonly FrameMetricsService _metricsService;

        public CSVParserService(FrameMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        /// <summary>
        /// Parses CSV file containing multiple 32x32 frames
        /// Returns list of frame data arrays ready for processing
        /// </summary>
        public async Task<CSVParseResult> ParseCSVFileAsync(Stream fileStream)
        {
            var result = new CSVParseResult
            {
                Success = false,
                Frames = new List<ParsedFrame>()
            };

            try
            {
                using (var reader = new StreamReader(fileStream))
                {
                    var allRows = new List<string[]>();
                    string? line;
                    int lineNumber = 0;

                    // Read entire CSV into memory
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;

                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Split by comma and trim whitespace
                        var values = line.Split(',')
                                        .Select(v => v.Trim())
                                        .ToArray();

                        // Validate: must have exactly 32 columns
                        if (values.Length != 32)
                        {
                            result.ErrorMessage = $"Invalid CSV format at line {lineNumber}: Expected 32 columns, found {values.Length}";
                            return result;
                        }

                        allRows.Add(values);
                    }

                    // Validate: total rows must be divisible by 32
                    if (allRows.Count % 32 != 0)
                    {
                        result.ErrorMessage = $"Invalid CSV format: Total rows ({allRows.Count}) is not divisible by 32. Each frame must be exactly 32 rows.";
                        return result;
                    }

                    int totalFrames = allRows.Count / 32;
                    result.TotalFrames = totalFrames;

                    // Extract frames (every 32 rows = one frame)
                    for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
                    {
                        try
                        {
                            // Get 32 rows for this frame
                            var frameRows = allRows.Skip(frameIndex * 32).Take(32).ToList();

                            // Parse into 1024-element array
                            var frameData = _metricsService.ParseFrameFromCSV(frameRows);

                            // Create parsed frame object
                            var parsedFrame = new ParsedFrame
                            {
                                FrameIndex = frameIndex,
                                Data = frameData,
                                RowStart = frameIndex * 32 + 1,
                                RowEnd = (frameIndex + 1) * 32
                            };

                            result.Frames.Add(parsedFrame);
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = $"Error parsing frame {frameIndex + 1} (rows {frameIndex * 32 + 1}-{(frameIndex + 1) * 32}): {ex.Message}";
                            return result;
                        }
                    }

                    result.Success = true;
                    result.Message = $"Successfully parsed {totalFrames} frames from CSV";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading CSV file: {ex.Message}";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Validates CSV file before processing
        /// </summary>
        public async Task<CSVValidationResult> ValidateCSVAsync(Stream fileStream, string fileName)
        {
            var result = new CSVValidationResult
            {
                IsValid = false,
                FileName = fileName
            };

            try
            {
                // Check file extension
                if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add("File must be a CSV file (.csv extension)");
                    return result;
                }

                // Check file size (max 50MB)
                if (fileStream.Length > 50 * 1024 * 1024)
                {
                    result.Errors.Add("File size exceeds 50MB limit");
                    return result;
                }

                // Check if file is empty
                if (fileStream.Length == 0)
                {
                    result.Errors.Add("File is empty");
                    return result;
                }

                // Read first few lines to validate format
                fileStream.Position = 0; // Reset stream
                using (var reader = new StreamReader(fileStream, leaveOpen: true))
                {
                    var firstLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(firstLine))
                    {
                        result.Errors.Add("CSV file appears to be empty");
                        return result;
                    }

                    // Check first line has 32 columns
                    var columns = firstLine.Split(',');
                    if (columns.Length != 32)
                    {
                        result.Errors.Add($"Invalid format: Expected 32 columns, found {columns.Length}");
                        return result;
                    }

                    // Try to parse first value to ensure it's numeric
                    if (!int.TryParse(columns[0].Trim(), out _))
                    {
                        result.Errors.Add("First column does not contain valid numeric data");
                        return result;
                    }

                    // Count total lines (for info)
                    int lineCount = 1;
                    while (await reader.ReadLineAsync() != null)
                    {
                        lineCount++;
                    }

                    result.TotalRows = lineCount;
                    result.EstimatedFrames = lineCount / 32;

                    // Warn if not divisible by 32
                    if (lineCount % 32 != 0)
                    {
                        result.Warnings.Add($"Total rows ({lineCount}) is not divisible by 32. File may be incomplete.");
                    }
                }

                // Reset stream for actual processing
                fileStream.Position = 0;

                result.IsValid = result.Errors.Count == 0;
                result.FileSizeMB = fileStream.Length / (1024.0 * 1024.0);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error validating file: {ex.Message}");
            }

            return result;
        }
    }

    // Helper classes for CSV parsing results
    public class CSVParseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalFrames { get; set; }
        public List<ParsedFrame> Frames { get; set; } = new();
    }

    public class ParsedFrame
    {
        public int FrameIndex { get; set; }
        public int[] Data { get; set; } = Array.Empty<int>();
        public int RowStart { get; set; }
        public int RowEnd { get; set; }
    }

    public class CSVValidationResult
    {
        public bool IsValid { get; set; }
        public string FileName { get; set; } = string.Empty;
        public double FileSizeMB { get; set; }
        public int TotalRows { get; set; }
        public int EstimatedFrames { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}