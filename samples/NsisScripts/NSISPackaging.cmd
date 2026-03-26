@echo off
setlocal enabledelayedexpansion

:: 进入脚本所在目录
cd /d "%~dp0"

:: 指定打包脚本
set "TARGET_NSI=%~1"
if not "%TARGET_NSI%"=="" (
    if not exist "%TARGET_NSI%" (
        echo [错误] 找不到指定的脚本文件: %TARGET_NSI%
        exit /b 1
    )
	echo [指定模式]
    call :Packing "%TARGET_NSI%"
    goto :end
)

:: 遍历所有打包脚本
echo [遍历模式]
for %%i in ("%~dp0*.nsi") do (
    call :Packing "%%~nxi"
)
goto :end

:: --- 打包 ---
:Packing
echo ---------------------------------------
echo 正在打包: %~1
".\NSIS\makensis.exe" "%~1"
if %errorlevel% neq 0 echo [警告] %~1 打包失败！
goto :eof

:: --- 结束 ---
:end
echo ------------------[END]------------------
echo 完成，以打包至: %~dp0
exit /b 0