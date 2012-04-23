$createMode = [System.IO.FileMode]::Create
$compressMode = [System.IO.Compression.CompressionMode]::Compress

foreach( $dll in ls "original" )
{
    $assembly = [System.IO.File]::ReadAllBytes( $dll.FullName )
    $fileOutputName = "compressed\" + $dll.Name + ".deflate"

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

