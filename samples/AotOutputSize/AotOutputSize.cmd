@echo off
setlocal enabledelayedexpansion

:: 参数处理
set "PROJ=%~1"
set "RID=%~2"
set "KEEP_LOG=%~3"

if "%RID%"=="" set "RID=win-x64"
if "%KEEP_LOG%"=="" set "KEEP_LOG=n"

if "%PROJ%"=="" (
    echo [错误] 必须提供项目路径。
    echo 用法：%~dp0%~n0.cmd <项目路径.csproj> [RID] [是否保留日志:y/n]
    echo 例如：%~dp0%~n0.cmd %~dp0\Hello\Hello.csproj win-x86 n
    exit /b 1
)

:: 路径初始化
set "BASE_DIR=%~dp0"
set "OUT_DIR=%BASE_DIR%AotOutput"
set "LOG_FILE=%BASE_DIR%AotBuildLog.txt"
if exist "%LOG_FILE%" del /q "%LOG_FILE%"

:: 清理
set "CLEAN_CMD=dotnet clean "%PROJ%" -c Release -r %RID%"
echo =========================================================== >> "%LOG_FILE%"
echo [阶段 0] 清理 >> "%LOG_FILE%"
echo 时间: !date! !time! >> "%LOG_FILE%"
echo 命令: !CLEAN_CMD! >> "%LOG_FILE%"
echo ----------------------------------------------------------- >> "%LOG_FILE%"
!CLEAN_CMD! >> "%LOG_FILE%" 2>&1
echo. >> "%LOG_FILE%"

:: 还原
set "RESTORE_CMD=dotnet restore "%PROJ%" -r %RID%"
echo =========================================================== >> "%LOG_FILE%"
echo [阶段 0] 还原 >> "%LOG_FILE%"
echo 时间: !date! !time! >> "%LOG_FILE%"
echo 命令: !RESTORE_CMD! >> "%LOG_FILE%"
echo ----------------------------------------------------------- >> "%LOG_FILE%"
!RESTORE_CMD! >> "%LOG_FILE%" 2>&1
echo. >> "%LOG_FILE%"

:: 配置矩阵
set "desc[1]=Base AOT Publish"            & set "conf[1]="
set "desc[2]=OptimizationPreference=size" & set "conf[2]=-p:OptimizationPreference=size"
set "desc[3]=OptimizationPreference=speed"& set "conf[3]=-p:OptimizationPreference=speed"
set "desc[4]=InvariantGlobalization=true" & set "conf[4]=-p:InvariantGlobalization=true"
set "desc[5]=DebuggerSupport=false"       & set "conf[5]=-p:DebuggerSupport=false"
set "desc[6]=StackTraceSupport=false"     & set "conf[6]=-p:StackTraceSupport=false"
set "desc[7]=UseSizeOptimizedLinq=true"   & set "conf[7]=-p:UseSizeOptimizedLinq=true"
set "desc[8]=UseSystemResourceKeys=true"  & set "conf[8]=-p:UseSystemResourceKeys=true"
set "desc[9]=Full Optimizations"          & set "conf[9]=-p:OptimizationPreference=size -p:InvariantGlobalization=true -p:DebuggerSupport=false -p:StackTraceSupport=false -p:UseSizeOptimizedLinq=true -p:UseSystemResourceKeys=true"

echo ==============================================================================
echo  Project: [%PROJ%] ^| RID: %RID%
echo ==============================================================================
echo Description                    ^| Size (Bytes)    ^| Duration (s)
echo ------------------------------------------------------------------------------

:: 基准测试循环
for /L %%i in (1,1,9) do (
    if exist "%OUT_DIR%" rd /s /q "%OUT_DIR%"
    mkdir "%OUT_DIR%"

    :: 构建命令：强制重新发布参数
    set "EXEC_ARGS=!conf[%%i]!"
    set "CMD_STR=dotnet publish "%PROJ%" -c Release -r %RID% -o "%OUT_DIR%" --force -p:PublishAot=true -p:IncrementalPackaging=false !EXEC_ARGS!"

    :: 写入日志
    echo =========================================================== >> "%LOG_FILE%"
    echo [阶段 %%i] !desc[%%i]! >> "%LOG_FILE%"
    echo 时间: !date! !time! >> "%LOG_FILE%"
    echo 命令: !CMD_STR! >> "%LOG_FILE%"
    echo ----------------------------------------------------------- >> "%LOG_FILE%"

    :: 执行命令并重定向输出
    set "STIME=!time!"
    !CMD_STR! >> "%LOG_FILE%" 2>&1
    set "ETIME=!time!"
    echo. >> "%LOG_FILE%"

    :: 计算耗时
    call :GetDuration "!STIME!" "!ETIME!" DURATION

    :: 统计输出文件夹内所有dll文件的总大小
    set "TOTAL_SIZE=0"
    for /r "%OUT_DIR%" %%f in (*.dll) do (
        set /a "TOTAL_SIZE+=%%~zf"
    )

    :: 表格对齐
    set "RAW_DESC=!desc[%%i]!                              "
    set "PAD_DESC=!RAW_DESC:~0,30!"
    set "RAW_SIZE=!TOTAL_SIZE!                "
    set "PAD_SIZE=!RAW_SIZE:~0,15!"

    echo !PAD_DESC! ^| !PAD_SIZE! ^| !DURATION!
)
echo ------------------------------------------------------------------------------

:: 清理
if exist "%OUT_DIR%" rd /s /q "%OUT_DIR%"
if /i "%KEEP_LOG%" neq "y" (
    if exist "%LOG_FILE%" del /q "%LOG_FILE%"
) else (
    echo.
    echo 详细编译日志已保存至: "%LOG_FILE%"
)

echo 完成。
exit /b 0

:: ================================================================
:: 函数: 计算时间差
:: ================================================================
:GetDuration
set "t1=%~1"
set "t2=%~2"
set "t1=%t1: =0%"
set "t2=%t2: =0%"
set /a "h1=1!t1:~0,2!-100, m1=1!t1:~3,2!-100, s1=1!t1:~6,2!-100, c1=1!t1:~9,2!-100"
set /a "h2=1!t2:~0,2!-100, m2=1!t2:~3,2!-100, s2=1!t2:~6,2!-100, c2=1!t2:~9,2!-100"
set /a "start_total=(h1*360000)+(m1*6000)+(s1*100)+c1"
set /a "end_total=(h2*360000)+(m2*6000)+(s2*100)+c2"
set /a "diff=end_total-start_total"
if %diff% lss 0 set /a "diff+=8640000"
set "ms=!diff:~-2!"
set "sec=!diff:~0,-2!"
if "!sec!"=="" set "sec=0"
set "%~3=!sec!.!ms!"
goto :eof
