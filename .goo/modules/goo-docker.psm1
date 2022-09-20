<# GooDocker.psm1 #>

class GooDocker {

    [string]$Name
    [Object]$Goo

    GooDocker( [Object]$goo ){
        $this.Name = "Docker"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [void] Up() {
        $this.Up((Get-Location).Path) 
    }

    [void] Up( [string]$folder ) {
        $this.Goo.Command.RunExternal( 'docker-compose', "up -d", $folder ) 
    }

    [void] Up([string]$folder, [string] $override) {
        $this.Goo.Command.RunExternal( 'docker-compose', "-f .\docker-compose.yaml -f $override up -d", $folder ) 
    }

    [void] Down(){
        $this.Down((Get-Location).Path)
    }

    [void] Down( [string]$folder ) {
        if ($null -ne (docker-compose --project-directory $folder ps -q)) {
            $this.Goo.Command.RunExternal( 'docker-compose', "down", $folder ) 
        }   
    }

    [void] StopAllRunningContainers()
    {
        $containers = $(docker ps -q)
        if($containers.count -gt 0){ docker kill $containers }
    }

}
