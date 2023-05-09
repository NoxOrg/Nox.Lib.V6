<# GooGit.psm1 #>

class GooGit {

    [string]$Name
    [Object]$Goo

    GooGit( [Object]$goo ){
        $this.Name = "Git"
        $this.Goo = $goo
    }

    [string] ToString()
    {
        return "Goo"+$this.Name+"Module"
    }

    [void] CheckoutFeature([string]$featureName)
    {
        git checkout -b feature/$featureName
        if($?) { git push --set-upstream origin feature/$featureName }
        if($?) { git pull }
    }

    [void] CheckoutMain()
    {
        $headBranch = $this.HeadBranch()
        $this.Checkout($headbranch)
    }

    [void] CheckoutMaster()
    {
        $this.CheckoutMain()
    }

    [void] Checkout($branch)
    {
        git checkout $branch
        if($?) { git pull --prune }
        if($?) { git fetch origin }
        if($?) { git reset --hard origin/$branch }
        if($?) { git clean -f -d }
    }

    [void] AddCommitPushRemote($message)
    {
        git add -A 
        if($?) { git commit -m $message }
        if($?) { git push -u origin }
    }

    [string] CurrentBranch()
    {
        return $(git branch --show-current)
    }

    [string] HeadBranch()
    {
        return $($(git remote show origin | Select-String -Pattern "HEAD branch: " -Raw -SimpleMatch -CaseSensitive) -replace "HEAD branch: ", "").Trim()
    }
}

