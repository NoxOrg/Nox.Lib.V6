<# goo.psm1 #>

using module '.\modules\goo-io.psm1'
using module '.\modules\goo-command.psm1'
using module '.\modules\goo-console.psm1'
using module '.\modules\goo-choco.psm1'
using module '.\modules\goo-network.psm1'
using module '.\modules\goo-sql.psm1'
using module '.\modules\goo-dotnet.psm1'
using module '.\modules\goo-benchmark.psm1'
using module '.\modules\goo-git.psm1'
using module '.\modules\goo-docker.psm1'
using module '.\modules\goo-version.psm1'

<# GOO CLASS IMPLEMENTATION #>

class Goo {

    [string]$Arguments
    
    [GooIO]$IO
    [GooConsole]$Console
    [GooCommand]$Command
    [GooChoco]$Choco
    [GooNetwork]$Network
    [GooSql]$Sql
    [GooDotnet]$Dotnet
    [GooBenchmark]$Benchmark
    [GooGit]$Git
    [GooDocker]$Docker
    [GooVersion]$Version

    Goo( [Object[]]$arguments ){
        
        $this.Arguments = $arguments

        # install modules
        $this.IO = [GooIO]::new( $this )
        $this.Console = [GooConsole]::new( $this )
        $this.Command = [GooCommand]::new( $this, $arguments )
        $this.Choco = [GooChoco]::new( $this )
        $this.Network = [GooNetwork]::new( $this )
        $this.Sql = [GooSql]::new( $this )
        $this.Dotnet = [GooDotnet]::new( $this )
        $this.Benchmark = [GooBenchmark]::new( $this )
        $this.Git = [GooGit]::new( $this )
        $this.Docker = [GooDocker]::new( $this )
        $this.Version = [GooVersion]::new( $this )

    }


    [void] Start() {

        $startTime = $(get-date)
        $oldProgressPreference = $global:ProgressPreference
        $global:ProgressPreference = "SilentlyContinue"
        $this.Console.WriteBanner()
        
        try {

            switch ($this.Command.MainCommand)
            {
                "help"             { $this.Console.WriteHelp(); $this.Version.GooGetVersion(); return; }
                "goo-version"      { $this.Version.GooGetVersion($true) ; return; }
                "goo-bump-build"   { $this.Version.GooBumpVersion('build') ; return; }
                "goo-bump-patch"   { $this.Version.GooBumpVersion('patch') ; return; }
                "goo-bump-minor"   { $this.Version.GooBumpVersion('minor') ; return; }
                "goo-bump-major"   { $this.Version.GooBumpVersion('major') ; return; }
                "goo-release"      { $this.Version.GooRelease(); return; }
                "goo-update"       { $this.Version.GooUpdate(); return; }
            }

            try {
                $this.Console.WriteInfo("Environment is [$Env:ENVIRONMENT]")
                $this.Command.Run()
                $elapsedTime = $(get-date) - $startTime
                $totalTime = "{0:HH:mm:ss}" -f ([datetime]$elapsedTime.Ticks)
                $this.Console.WriteInfo("Success! ($totalTime)")
        
            } catch {
                Pop-Location
                $this.Console.WriteError("Error - $PSItem :(")
            }
    
        }
        finally {
            $global:ProgressPreference = $oldProgressPreference
        }
    }

    [void] Error( [string]$message ) {
        throw $message
    }

    [void] StopIfError( [string]$message ) {
        if ($this.Command.LastExitCode -gt 0) {
            $this.Error($message)
        }
    }

    [bool] IsAdminSession() {
        return ([Security.Principal.WindowsPrincipal] `
        [Security.Principal.WindowsIdentity]::GetCurrent()
       ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }

    [void] Sleep( [int]$seconds ) {
        Start-Sleep -Seconds $seconds
    }

}



