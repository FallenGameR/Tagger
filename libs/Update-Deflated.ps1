$scriptLocation = Split-Path $MyInvocation.MyCommand.Path
$referenced =
    "$scriptLocation\WpfNotifyIcon\Hardcodet.Wpf.TaskbarNotification.dll",
    "$scriptLocation\ManagedWinAPI\ManagedWinapi.dll",
    "$scriptLocation\Prism\Microsoft.Practices.Prism.dll",
    "$scriptLocation\..\src\Tagger.Lib\bin\Debug\Tagger.Lib.dll",
    "$scriptLocation\ExtendedWPFToolkit\WPFToolkit.Extended.dll"

$createMode = [System.IO.FileMode]::Create
$compressMode = [System.IO.Compression.CompressionMode]::Compress

foreach( $path in $referenced )
{
    $dll = Get-Item $path
    $assembly = [System.IO.File]::ReadAllBytes( $dll.FullName )
    $fileOutputName = "$scriptLocation\Deflated\compressed\" + $dll.Name + ".deflate"

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

