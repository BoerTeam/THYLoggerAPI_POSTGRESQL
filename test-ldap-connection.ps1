# LDAP Bağlantı Test Scripti
# Bu script ile LDAP ayarlarınızı test edebilirsiniz

Write-Host "=== LDAP Bağlantı Testi ===" -ForegroundColor Cyan
Write-Host ""

# Test parametreleri
$ldapServer = "192.168.0.17"
$ldapPort = 389
$username = "administrator"
$domain = "boerltd.localhost"

Write-Host "LDAP Sunucu: $ldapServer" -ForegroundColor Yellow
Write-Host "Port: $ldapPort" -ForegroundColor Yellow
Write-Host "Domain: $domain" -ForegroundColor Yellow
Write-Host ""

# 1. Network bağlantısı testi
Write-Host "1. Network bağlantısı test ediliyor..." -ForegroundColor Green
try {
	$connection = Test-NetConnection -ComputerName $ldapServer -Port $ldapPort -WarningAction SilentlyContinue
	if ($connection.TcpTestSucceeded) {
		Write-Host "   ✓ Sunucuya erişim başarılı (Port $ldapPort açık)" -ForegroundColor Green
	} else {
		Write-Host "   ✗ Sunucuya erişim başarısız!" -ForegroundColor Red
		exit
	}
} catch {
	Write-Host "   ✗ Bağlantı hatası: $($_.Exception.Message)" -ForegroundColor Red
	exit
}

Write-Host ""

# 2. Active Directory modülü kontrolü
Write-Host "2. Active Directory modülü kontrol ediliyor..." -ForegroundColor Green
if (Get-Module -ListAvailable -Name ActiveDirectory) {
	Import-Module ActiveDirectory
	Write-Host "   ✓ ActiveDirectory modülü yüklü" -ForegroundColor Green
} else {
	Write-Host "   ! ActiveDirectory modülü yüklü değil (opsiyonel)" -ForegroundColor Yellow
}

Write-Host ""

# 3. Domain bilgilerini al
Write-Host "3. Domain bilgileri alınıyor..." -ForegroundColor Green
try {
	if (Get-Module -Name ActiveDirectory) {
		$adDomain = Get-ADDomain -Server $ldapServer
		Write-Host "   Domain Name: $($adDomain.DNSRoot)" -ForegroundColor Cyan
		Write-Host "   NetBIOS Name: $($adDomain.NetBIOSName)" -ForegroundColor Cyan
		Write-Host "   Distinguished Name: $($adDomain.DistinguishedName)" -ForegroundColor Cyan
		Write-Host "   Domain Controllers: $($adDomain.ReplicaDirectoryServers -join ', ')" -ForegroundColor Cyan

		Write-Host ""
		Write-Host "   *** appsettings.json için önerilen BaseDn: ***" -ForegroundColor Yellow
		Write-Host "   ""BaseDn"": ""$($adDomain.DistinguishedName)""" -ForegroundColor White
	}
} catch {
	Write-Host "   ! Domain bilgisi alınamadı: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

# 4. Kullanıcı testi (şifre ile)
Write-Host "4. Kullanıcı kimlik doğrulama testi" -ForegroundColor Green
Write-Host "   Kullanıcı adı: $username" -ForegroundColor Cyan

$password = Read-Host "   Şifrenizi girin" -AsSecureString

try {
	# UPN formatı ile test
	$upn = "$username@$domain"
	Write-Host ""
	Write-Host "   Test 1: UPN formatı ($upn) ile..." -ForegroundColor Yellow

	Add-Type -AssemblyName System.DirectoryServices.Protocols
	$ldapConnection = New-Object System.DirectoryServices.Protocols.LdapConnection("$ldapServer`:$ldapPort")
	$ldapConnection.AuthType = [System.DirectoryServices.Protocols.AuthType]::Basic

	$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
	$plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
	[System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)

	$credential = New-Object System.Net.NetworkCredential($upn, $plainPassword)
	$ldapConnection.Credential = $credential
	$ldapConnection.Bind()

	Write-Host "   ✓ Kimlik doğrulama BAŞARILI!" -ForegroundColor Green
	Write-Host ""
	Write-Host "   *** appsettings.json için önerilen ayarlar: ***" -ForegroundColor Yellow
	Write-Host "   ""Domain"": ""$domain""" -ForegroundColor White
	Write-Host "   Login formatı: $username veya $upn" -ForegroundColor White

} catch {
	Write-Host "   ✗ Kimlik doğrulama BAŞARISIZ!" -ForegroundColor Red
	Write-Host "   Hata: $($_.Exception.Message)" -ForegroundColor Red

	# Alternatif format dene
	Write-Host ""
	Write-Host "   Test 2: Domain\User formatı ile deneniyor..." -ForegroundColor Yellow
	try {
		$domainUser = "boerltd\$username"
		$credential2 = New-Object System.Net.NetworkCredential($domainUser, $plainPassword)
		$ldapConnection2 = New-Object System.DirectoryServices.Protocols.LdapConnection("$ldapServer`:$ldapPort")
		$ldapConnection2.AuthType = [System.DirectoryServices.Protocols.AuthType]::Basic
		$ldapConnection2.Credential = $credential2
		$ldapConnection2.Bind()

		Write-Host "   ✓ Domain\User formatı ile BAŞARILI!" -ForegroundColor Green
		Write-Host "   Login formatı: $domainUser" -ForegroundColor White
	} catch {
		Write-Host "   ✗ Bu format da başarısız: $($_.Exception.Message)" -ForegroundColor Red
	}
}

Write-Host ""
Write-Host "=== Test Tamamlandı ===" -ForegroundColor Cyan
