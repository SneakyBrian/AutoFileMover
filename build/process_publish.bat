@echo off

pandoc -f html -t markdown-simple_tables-pipe_tables -o index_temp.md ..\downloads\latest\publish.html

copy header.md + index_temp.md ..\downloads\latest\index.md"

del index_temp.md

pause