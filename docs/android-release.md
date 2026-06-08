# Android Release Checklist

This file keeps the release steps repeatable without storing signing secrets in Git.

## 1. Required Local Files

- Keystore: `E:\Development\WordsTrainer\wordstrainer-release.keystore`
- Alias: `wordstrainer`
- Store password: keep outside Git
- Key password: keep outside Git

Never commit `*.keystore`, `*.jks`, `*.apk`, `*.aab`, or `*.idsig`.

## 2. Build Release APK For Manual Testing

```powershell
dotnet publish WordsTrainer.Mobile/WordsTrainer.Mobile.csproj `
  -f net10.0-android `
  -c Release `
  -p:AndroidPackageFormat=apk `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="E:\Development\WordsTrainer\wordstrainer-release.keystore" `
  -p:AndroidSigningKeyAlias="wordstrainer" `
  -p:AndroidSigningKeyPass="YOUR_KEY_PASSWORD" `
  -p:AndroidSigningStorePass="YOUR_STORE_PASSWORD"
```

If the generated signed APK does not verify, sign manually:

```powershell
$env:JAVA_HOME = "C:\Program Files\Android\openjdk\jdk-21.0.8"
$buildTools = "C:\Program Files (x86)\Android\android-sdk\build-tools\36.0.0"

$unsigned = "E:\Development\WordsTrainer\WordsTrainer.Mobile\bin\Release\net10.0-android\publish\com.wordstrainer.app.apk"
$aligned = "E:\Development\WordsTrainer\WordsTrainer.Mobile\bin\Release\net10.0-android\publish\com.wordstrainer.app-aligned.apk"
$signed = "E:\Development\WordsTrainer\WordsTrainer.Mobile\bin\Release\net10.0-android\publish\com.wordstrainer.app-release.apk"

& "$buildTools\zipalign.exe" -f -p 4 $unsigned $aligned

& "$buildTools\apksigner.bat" sign `
  --ks "E:\Development\WordsTrainer\wordstrainer-release.keystore" `
  --ks-key-alias wordstrainer `
  --out $signed `
  $aligned

& "$buildTools\apksigner.bat" verify --verbose $signed
```

Install on a physical device:

```powershell
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" `
  -s R58M41Z9XTW `
  install -r "E:\Development\WordsTrainer\WordsTrainer.Mobile\bin\Release\net10.0-android\publish\com.wordstrainer.app-release.apk"
```

## 3. Build AAB For Google Play

```powershell
dotnet publish WordsTrainer.Mobile/WordsTrainer.Mobile.csproj `
  -f net10.0-android `
  -c Release `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="E:\Development\WordsTrainer\wordstrainer-release.keystore" `
  -p:AndroidSigningKeyAlias="wordstrainer" `
  -p:AndroidSigningKeyPass="YOUR_KEY_PASSWORD" `
  -p:AndroidSigningStorePass="YOUR_STORE_PASSWORD"
```

Expected output folder:

```text
WordsTrainer.Mobile\bin\Release\net10.0-android\publish
```

## 4. Smoke Test Before Upload

- App opens from launcher.
- Login works.
- Training loads and accepts answers.
- Explanation page opens.
- Forgot/reset password still works.
- Localization follows selected native language.
- Notification permission prompt is acceptable.
- Daily reminder behavior is checked after 17:00.

## 5. Play Console Assets

- App name: `WordsTrainer`
- Package name: `com.wordstrainer.app`
- Version name: `1.0.0`
- Version code: `1`
- Privacy policy URL: `https://YOUR-WEB-DOMAIN/privacy`
- Category: Education
- Short description: `Daily vocabulary practice for language learners.`
- Full description:

```text
WordsTrainer helps language learners practice vocabulary through short daily training sessions.
The app mixes new words with review words, tracks progress, supports localized UI texts, and
offers password reset through email.
```

Prepare screenshots from the release APK on a real device before uploading.
