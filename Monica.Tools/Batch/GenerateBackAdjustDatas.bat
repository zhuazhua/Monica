@echo off
cd %~dp0
cd ../Release
Monica.Tools -a GenerateBackAdjustDatas -i <inDir> -o <outDir>
pause