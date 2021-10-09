# Blast CMS

A fun Headless CMS that uses MartenDb and Blazor Server.

## Secrets

You will need to add the following secrets for this project to work.  If you are using visual studio, I recommend using the manage secrets option instead of adding these to your `appsettings.json` or `launchsettings.json` file.

```json
{
  "TINIFY_API_KEY": "register on https://tinypng.com/developers to get our free key",
  "Kestrel:Certificates:Development:Password": "kestrel cert key for local development",
  "Auth0": {
    "Domain": "your domain",
    "ClientId": "your client id",
    "ClientSecret": "your client secret"
  }
}
```

You will also need to [create a google credentials file](https://cloud.google.com/dotnet/docs/setup) and set the path in the `launchsettings.json` file.

```json
"GoogleCredentialFile": "c:/data/temp/bradjolicoeur-web-f75a278433e9.json"
```

## External Service Dependencies

- [Auth0](https://auth0.com/)
  - Authentication
- [TinyPNG](https://tinypng.com/developers)
  - Optimize image sizes during upload
- [Google Cloud Storage](https://cloud.google.com/storage)
  - Storage of images
