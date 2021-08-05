This project contains 3 logical parts.
1. An Azure function to process files from Azure File Share every day at 6 PM. 
2. A StreamProcessor type to read the lines from remote file using streams
3. A Pattern matching logic compares the line to that of a pattern set in function's localsettings.config file
