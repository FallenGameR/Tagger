$root = $myInvocation.MyCommand.Path -replace "(?<=Tagger).+"
$injector = Join-Path $root "bin\Debug\Injector.exe"
$hook = Join-Path $root "bin\Debug\Hook.dll"

Describe "Injector" {

	It "binary should exist" {
		(Test-Path $injector).should.be( $true )
	}

	It "should print usage help on call without arguments" {
		(& $injector).should.match( "syntax" )
	}
	
	It "should inject test hook dll into cmd" {
		$cmd = Start-Process cmd -WindowStyle Hidden -PassThru
		$injector_output = & $injector $cmd.Id $hook
		$injector_output.should.match( "Dll was successfully injected" )
		
		$cmd.Refresh()
		$present_modules = $cmd.Modules | where{$_.FileName -eq $hook}
		$present_modules.should.have_count_of( 1 )
		$cmd.Kill()
	}
}
