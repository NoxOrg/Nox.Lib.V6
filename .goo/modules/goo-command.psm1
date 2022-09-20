<# GooCommand.psm1 #>

class GooCommand {

    [string]$Name
    [Object]$Goo

    [hashtable]$Commands = @{}
    [Object[]]$Arguments
    [string]$MainCommand
    [int]$LastExitCode

    GooCommand( [Object]$goo, [Object[]]$arguments ) {
        $this.Goo = $goo
        $this.Name = "Command"
        # parse arguments
        if( $arguments.Count -eq 0 ) {
            $this.MainCommand = 'help'
            $this.Arguments = @()
        } elseif ( $arguments.Count -eq 1 ) {
            $this.MainCommand = $arguments[0].ToLower()
            $this.Arguments = @()
        } else {
            $this.MainCommand, $this.Arguments = $arguments;
            $this.MainCommand = $this.MainCommand.ToLower()
        }
    }

    [string] ToString() {
        return "Goo"+$this.Name+"Module"
    }

    [void] Add([string]$name, [ScriptBlock]$scriptBlock ) {
        ## Add argument validation logic here
        $this.Commands.Add( $name.ToLower(), $scriptBlock )
    }    

    [void] Run() {
        $this.Run( $this.MainCommand, $this.Arguments )
    }
    
    [void] Run( [string]$command ) {
        $this.Run( $command, $this.Arguments )
    }

    [void] Run( [string]$command, [Object[]] $arguments ) {
        if ( -not ($this.Commands.ContainsKey($command) ) ) {
            $this.Goo.Error("No command '$command' is defined in goo file.")
        }
        Invoke-Command -ScriptBlock $this.Commands[$command] -ArgumentList $arguments
    }

    [bool] RunExternal( [string]$cmd, [Object]$parameters ) {
        if( $null -eq $parameters ) {
            $parameters = ""
        }
        $process = Start-Process $cmd $parameters -NoNewWindow -ErrorAction Stop -PassThru
        Wait-Process -InputObject $process
        $this.LastExitCode = $process.ExitCode
        return ($this.LastExitCode -eq 0)
    }
    
    [bool] RunExternal( [string]$cmd ) {
        return $this.RunExternal( $cmd, $null )
    }

    [bool] RunExternal( [string]$cmd, [string]$parameters, [string]$inFolder ) {
        Push-Location $inFolder
        $ret = $this.RunExternal( $cmd, $parameters )
        Pop-Location
        return $ret
    }

    [psObject] RunExternalExt([string]$cmd, [Object]$parameters) {
        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = $cmd
        $pinfo.RedirectStandardError = $true
        $pinfo.RedirectStandardOutput = $true
        $pinfo.UseShellExecute = $false
        $pinfo.Arguments = $parameters
        $p = New-Object System.Diagnostics.Process
        $p.StartInfo = $pinfo
        $p.Start() | Out-Null
        $p.WaitForExit()
        $stdout = $p.StandardOutput.ReadToEnd()
        $stderr = $p.StandardError.ReadToEnd()
        
        $obj = New-Object PSObject -Property @{
            Output = $stdout
            Error = $stderr
            ExitCode = $p.ExitCode
        }
        return $obj
    }

    [void] StartProcess( [string]$cmd, [Object]$parameters ) {
        if( $null -eq $parameters ) {
            Start-Process $cmd 
        } else {
            Start-Process $cmd -ArgumentList $parameters 
        }
    }

    [void] StartProcess( [string]$cmd ) {
        $this.StartProcess( $cmd, $null )
    }

    [bool] EnsureProcessStopped( [string]$process ) {
        if ($null -ne (Get-Process -ProcessName $process -ErrorAction SilentlyContinue)) { 
            Get-Process -ProcessName $process | ForEach-Object { $_.Kill() } 
            return $true
        } else {
            return $false
        }
    }

    [bool] RunBrowser( [string]$url ) {
        $browser = ''
        $registryPath = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths' 

        if (Test-Path "$registryPath\chrome.exe"){
            $browser = (Get-Item (Get-ItemProperty "$registryPath\chrome.exe")."(Default)")

        } elseif (Test-Path "$registryPath\msedge.exe") {
            $browser = (Get-Item (Get-ItemProperty "$registryPath\msedge.exe")."(Default)")
            
        } else {
            $this.Goo.Error("No installed browser (Edge or Chrome) found.")
        }
        Start-Process $browser -ArgumentList "/new-window $url"
        return $true
    }
}
