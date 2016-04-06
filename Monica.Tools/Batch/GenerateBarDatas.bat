@echo off
cd %~dp0
cd ../Release
Monica.Tools -a GenerateBarDatas -i <inDir> -o <outDir> -b <barSize>
pause 