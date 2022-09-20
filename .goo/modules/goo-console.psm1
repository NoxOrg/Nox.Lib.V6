<# GooConsole.psm1 #>

# Default theme


class GooConsole {

    [string]$Name
    [Object]$Goo

    hidden [string]$ForegroundColor = "DarkGray"
    hidden [string]$BackgroundColor = "Black"
    hidden [string]$HighlightColor = "Magenta"
    hidden [string]$ErrorForegroundColor = "White"
    hidden [string]$ErrorBackgroundColor = "DarkMagenta"
    
    GooConsole( [Object]$goo ) {
        $this.Goo = $goo
        $this.Name = "Console"
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [void] Write() {
        $this.Write( '', $this.ForegroundColor, $this.BackgroundColor) 
    }

    [void] Write( [Object]$object ) {
        $this.Write( $object, $this.ForegroundColor, $this.BackgroundColor) 
    }

    [void] Write( [Object]$object, [string]$fgColor ) {
        $this.Write( $object, $fgColor, $this.BackgroundColor) 
    }

    [void] Write( [Object]$object, [string]$fgColor, [string]$bgColor ) {
        Write-Host -ForegroundColor $fgColor -Object $object -NoNewline
    }

    [void] WriteHighlight( [Object]$object ) {
        $this.Write( $object, $this.HighlightColor, $this.BackgroundColor) 
    }

    [void] WriteLine() {
        $this.WriteLine( '', $this.ForegroundColor, $this.BackgroundColor) 
    }

    [void] WriteLine( [Object]$object ) {
        $this.WriteLine( $object, $this.ForegroundColor, $this.BackgroundColor) 
    }

    [void] WriteLine( [Object]$object, [string]$fgColor ) {
        $this.WriteLine( $object, $fgColor, $this.BackgroundColor) 
    }

    [void] WriteLine( [Object]$object, [string]$fgColor, [string]$bgColor ) {
        Write-Host -ForegroundColor $fgColor -BackgroundColor $bgColor -Object $object
    }

    [void] WriteLineHighlight( [Object]$object ) {
        $this.WriteLine( $object, $this.HighlightColor, $this.BackgroundColor) 
    }

    [void] WriteInfo( [Object]$object ) {
        $this.WriteLine()
        $this.WriteLine( " goo> "+$object.ToString(), $this.HighlightColor, $this.BackgroundColor) 
        $this.WriteLine()
    }

    [void] WriteError( [Object]$object ) {
        $width = $global:Host.UI.RawUI.WindowSize.Width
        $this.WriteLine("".PadRight($width), $this.ErrorForegroundColor, $this.ErrorBackgroundColor)
        $this.WriteLine( (" goo> "+$object.ToString()).PadRight($width), $this.ErrorForegroundColor, $this.ErrorBackgroundColor) 
        $this.WriteLine("".PadRight($width), $this.ErrorForegroundColor, $this.ErrorBackgroundColor)
    }

    [void] WriteBanner() {
        $this.WriteLine(' __ __ __') 
        $this.Write('/_//_//_/') 
        $this.WriteHighlight('  >  ') 
        $this.WriteLine("Goo v$($this.Goo.Version.Current) - Type less. Code more.") 
        $this.WriteLine('_/')
        $this.WriteInfo("Project folder is [$(Get-Location | Split-Path -Leaf)]")
    }

    [void] WriteHelp() {
        $this.WriteLine( "Help: -" )
        $p = "# command: "
        $cmds = Get-Content -Path $((Get-Location).Path + '\.goo.ps1') | Select-String -pattern $p 
        foreach ($cmd in $cmds) {
            $cmdname, $purpose = $cmd.ToString().Split('|')
            $this.Write($cmdname.Replace($p,'').Trim().PadRight(22))
            $this.WriteHighlight(' > ')
            $this.WriteLine($purpose.Trim())
        }
        $this.WriteLine() 
    }

}