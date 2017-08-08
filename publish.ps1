[string]$token = Read-Host -Prompt "what is the token";

$token = $token.Trim();

$packages = dir .\release\*.nupkg | %{ $_.Name };
$packages | %{
	.\.nuget\NuGet.exe push .\release\$_ $token -source https://nuget.org;
}