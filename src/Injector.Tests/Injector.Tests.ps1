$root = $myInvocation.MyCommand.Path -replace "(?<=Tagger).+"
$injector = Join-Path $root "bin\Debug\Injector.exe"
$hook = Join-Path $root "playground\HookSpy\Debug\HookSpyDll.dll"

Describe "Injector" {

	It "binary should exist" {
		(Test-Path $injector).should.be( $true )
	}

	It "should print usage help on call without arguments" {
		(& $injector).should.match( "syntax" )
	}
	
	It "should inject test hook dll into cmd" {
		$cmd = Start-Process cmd -WindowStyle Hidden -PassThru
		(& $injector $cmd.Id $hook).should.match( "Dll was successfully injected" )
		
		$cmd.Refresh()
		($cmd.Modules | where{$_.FileName -eq $hook}).should.have_count_of( 1 )
	}
}
