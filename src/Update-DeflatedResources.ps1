<#
.DESCRIPTION
    This script is called on Tagger.Lib build.
    It updates deflated referenced assemblies in resources for Tagger.Wpf.

    The script is very simple. It wouldn't detect new referenced assembly or
    tagger lib version update. You have to manually update library list, copy
    local referenced assembly property and full versioned name of tagger lib
    in assembly resolve mapping.

.LINKS
    http://msdn.microsoft.com/en-us/library/7k989cfy.aspx
#>

param
(
    [Parameter(Mandatory=$true)]
    [string] $TaggerLibPath
)

$scriptLocation = Split-Path $MyInvocation.MyCommand.Path
$referenced =
    "$scriptLocation\..\libs\WpfNotifyIcon\Hardcodet.Wpf.TaskbarNotification.dll",
    "$scriptLocation\..\libs\ManagedWinAPI\ManagedWinapi.dll",
    "$scriptLocation\..\libs\Prism\Microsoft.Practices.Prism.dll",
    "$scriptLocation\..\libs\ExtendedWPFToolkit\WPFToolkit.Extended.dll",
    $taggerLibPath

$createMode = [System.IO.FileMode]::Create
$compressMode = [System.IO.Compression.CompressionMode]::Compress

foreach( $path in $referenced )
{
    $dll = Get-Item $path
    $assembly = [System.IO.File]::ReadAllBytes( $dll.FullName )
    $fileOutputName = "$scriptLocation\Tagger.Wpf\Resources\" + $dll.Name + ".deflate"

    try
    {
        $file = [System.IO.File]::Open( $fileOutputName, $createMode )
        $stream = New-Object System.IO.Compression.DeflateStream $file, $compressMode
        $writer = New-Object System.IO.BinaryWriter $stream
        $writer.Write( $assembly )
    }
    finally
    {
        if( $writer -ne $null ) { $writer.Close() }
        if( $stream -ne $null ) { $stream.Close() }
        if( $file -ne $null ) { $file.Close() }
    }
}

