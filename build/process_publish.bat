@echo off

pandoc -f html -t markdown -o index_temp.md ..\downloads\latest\publish.html

copy header.md + index_temp.md ..\downloads\latest\index.md"

del index_temp.md

pause