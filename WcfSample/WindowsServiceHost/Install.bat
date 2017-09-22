%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe WindowsServiceHost.exe
Net Start LR
sc config LR start= auto