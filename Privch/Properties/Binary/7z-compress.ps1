$ErrorActionPreference = "Stop"
$7z = "D:\[Knight]\7-Zip\Console\x64\7za.exe"

function CompressAndDelete {
    Param ([string]$path, [string[]]$files)
    
    Set-Location -Path $path

    foreach($file in $files) {
        if (Test-Path -Path ".\$file") {
            Get-FileHash .\$file -Algorithm MD5 | Format-List
            & $7z a -tgzip "$file.gz" $file
            Remove-Item $file
        }
    }
}


#v2ray 4.22.1
$path = "F:\PrivCh-Windows\Source-PrivCh\PrivCh\Properties\Binary\v2ray"
$files = 
    "v2ctl.exe",
    "v2ray.exe"

CompressAndDelete -path $path -files $files


#shadowsocks 3.3.4
$path = "F:\PrivCh-Windows\Source-PrivCh\PrivCh\Properties\Binary\shadowsocks"
$files = 
    "cygev-4.dll",
    "cyggcc_s-seh-1.dll",
    "cygmbedcrypto-3.dll",
    "cygpcre-1.dll",
    "cygsodium-23.dll",
    "cygwin1.dll",
    "ss-local.exe"

CompressAndDelete -path $path -files $files


#privoxy 3.0.28
$path = "F:\PrivCh-Windows\Source-PrivCh\PrivCh\Properties\Binary\privoxy"
$files = 
    "privoxy.exe"
    
CompressAndDelete -path $path -files $files


#done
Read-Host "Complete. Press any key to close...";
