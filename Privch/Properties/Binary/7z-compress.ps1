$ErrorActionPreference = "Stop"
$7za = "D:\[Knight]\7-Zip\Console\x64\7za.exe"

function CompressAndDelete {
    Param ([string]$path, [string[]]$files)
    
    Set-Location -Path $path

    foreach($file in $files) {
        if (Test-Path -Path "$file") {
            # delete the old gz file
            if(Test-Path -Path "$file.gz") {
                Remove-Item "$file.gz"
            }

            # compress to new gz file
            Get-FileHash $file -Algorithm MD5 | Format-List
            & $7za a -tgzip "$file.gz" $file
            Remove-Item "$file"
        }
    }
}


#v2ray 4.22.1
$path = "F:\PrivCh-Windows\Source-PrivCh\PrivCh\Properties\Binary\v2ray"
$files = 
    "v2ctl.exe",
    "v2ray.exe"

CompressAndDelete -path $path -files $files


#shadowsocks 3.3.5
$path = "F:\PrivCh-Windows\Source-PrivCh\PrivCh\Properties\Binary\shadowsocks"
$files = 
    "libbloom.dll",
    "libcork.dll",
    "libev-4.dll",
    "libgcc_s_seh-1.dll",
    "libipset.dll",
    "libmbedcrypto.dll",
    "libpcre-1.dll",
    "libsodium-23.dll",
    "libwinpthread-1.dll",
    "ss-local.exe"

CompressAndDelete -path $path -files $files


#privoxy 3.0.29
$path = "F:\PrivCh-Windows\Source-PrivCh\PrivCh\Properties\Binary\privoxy"
$files = 
    "privoxy.exe"
    
CompressAndDelete -path $path -files $files


#done
Read-Host "Complete. Press any key to close ...";
