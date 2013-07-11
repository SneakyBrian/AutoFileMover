using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Support
{
    public class FileCopier
    {
        public static async Task CopyFile(string sourceFilePath, string destinationFilePath, Action<int> progressHandler)
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB buffer

            using (FileStream source = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;
                using (FileStream dest = new FileStream(destinationFilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    long totalBytes = 0;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytes += currentBlockSize;
                        int percentage = (int)(((double)totalBytes / (double)fileLength) * 100.0);

                        await dest.WriteAsync(buffer, 0, currentBlockSize);

                        if (progressHandler != null)
                        {
                            progressHandler(percentage);
                        }
                    }
                }
            }
        }
    }
}
