<# GooChoco.psm1 #>

class GooChoco {

    [string]$Name
    [Object]$Goo

    GooChoco( [Object]$goo ){
        $this.Name = "Choco"
        $this.Goo = $goo
    }

    [string] ToString() {
        return "Goo"+$this.Name+"Module"
    }

    [bool] EnsureAppInstalled( [string]$chocoPackage ) {
        
        $this.EnsureChocoInstalled()

        if( $this.IsAppInstalled( $chocoPackage ) -or ( choco list --local-only --limit-output | Where-Object { $_.ToLower().StartsWith($chocoPackage) } ).Count -gt 0 ) {
            $this.Goo.Console.WriteInfo("'$chocoPackage' is already installed..." )
            return $false

        } else {
            $this.Goo.Console.WriteInfo("Installing '$chocoPackage'..." )
            $chocoCommand = "choco"
            $chocoParams = "install $chocoPackage -y --force"
            $this.Goo.Command.RunExternal($chocoCommand,$chocoParams)
            $this.Goo.StopIfError( "Failed to install '$chocoPackage'." ) 
            return $true
        }
    }
    
    [bool] IsAppInstalled( [string]$chocoPackage ) {
        $installed = $false
        switch ($chocoPackage)
        {
            'git' { $installed = (git --version) ; Break}
            'dotnetcore-sdk' { $installed = (dotnet --list-sdks | Where-Object { $_.Contains('5.0.') } | Measure-Object).Count -gt 0; Break }
            'postman' {$installed = Test-Path -Path "$env:USERPROFILE\AppData\Local\Postman\Postman.exe"; Break }
            'docker-desktop' {$installed = Test-Path -Path 'C:\Program Files\Docker\Docker\Docker Desktop.exe' ; Break }
        }
        return $installed
    }

    [bool] EnsureChocoInstalled() {
        # $isChocoInstalled = powershell choco -v
        $isChocoInstalled = $null -ne $Env:ChocolateyInstall -and (Get-ChildItem "$Env:ChocolateyInstall\choco.exe")
        if($isChocoInstalled){
            return $false
        } else {
            $this.Goo.Console.WriteInfo('Chocolatey is not installed, installing now...')
            Set-ExecutionPolicy Bypass -Scope Process -Force; `
                [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; `
                Invoke-WebRequest https://chocolatey.org/install.ps1 -UseBasicParsing | `
                Invoke-Expression
            return $true
        }
    }
}
