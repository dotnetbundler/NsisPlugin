Unicode true
SetCompress off
!addplugindir ".\addplugin"

Name "插件测试程序"

Section
	UseLocalNsisPlugin::Add 100 99
	Pop $0
	DetailPrint $0
	
    MessageBox MB_OK "完成"
	
	; 将窗口移动到顶部居中
	UseLocalNsisPlugin::MoveWindow ct
SectionEnd