#http://jongurgul.com/blog/get-stringhash-get-filehash/
Function Get-StringHash([String] $String,$HashName = "MD5")
{
  $StringBuilder = New-Object System.Text.StringBuilder
  [System.Security.Cryptography.HashAlgorithm]::Create($HashName).ComputeHash([System.Text.Encoding]::UTF8.GetBytes($String))|%{
    [Void]$StringBuilder.Append($_.ToString("x2"))
  }
  $StringBuilder.ToString()
}

$ol = Get-Location |% Path
Set-Location Dist/Public

$t = "";
Get-ChildItem |% { 
    (Get-StringHash ($_.Name + ".hash") "SHA256") + (Get-FileHash -Algorithm SHA256 $_.Name |% Hash).ToLower()
} |% {
    $t = "$t$_";
}

$gt = "";
foreach ($c in $t.ToCharArray()) {
    $st = $c;
    0..(Get-Random -Maximum 3) |% {
        [char[]](Get-Random -Minimum 65 -Maximum 90)
    } |% {
        $st = "$st$_";
    }
    $gt = "$gt$st"
}

Set-Content -Path Info $gt -NoNewline

scp -r ..\Public vps:/var/www/html/KiraiMod/

Set-Location $ol
