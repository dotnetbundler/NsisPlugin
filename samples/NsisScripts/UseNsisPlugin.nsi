Unicode true
SetCompress off
!addplugindir ".\addplugin"

Name "插件测试程序"

Section
	DetailPrint "开始"
	
	UseNsisPlugin::Add 200 99
	Pop $0
	DetailPrint $0
	
    MessageBox MB_OK "完成"
SectionEnd