# TimerTrigger - C<span>#</span>

The `TimerTrigger` makes it incredibly easy to have your functions executed on a schedule. This sample demonstrates a simple use case of calling your function every 5 minutes.

## How it works

For a `TimerTrigger` to work, you provide a schedule in the form of a [cron expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)(See the link for full details). A cron expression is a string with 6 separate expressions which represent a given schedule via patterns. The pattern we use to represent every 5 minutes is `0 */5 * * * *`. This, in plain text, means: "When seconds is equal to 0, minutes is divisible by 5, for any hour, day of the month, month, day of the week, or year".

## Learn more

# 概要

Dropbox の指定パスに上がったファイルをメールで送信する AzureFunctions

# 設定

```json
{
    "Dropbox" : {
        "RefreshToken" : "{Dropbox RefreshToken}",
        "ApiKey" : "{Dropbox API Key}",
        "APiSecret" : "{Dropbox API Secret}",
        "Path" : "{監視パス}"
    },
    "Smtp" : {
        "Host" : "{Host}",
        "Port" : 465,
        "UseSSL": true,
        "Username" : "{SMTP ユーザ名}",
        "Password" : "{SMTP パスワード}"
    },
    "To" : "{送信先メールアドレス}"
}
```

## Dropbox トークン作成方法

1. https://www.dropbox.com/oauth2/authorize?client_id={APP_KEY}&response_type=code&token_access_type=offline をブラウザで開く

2. 取得したコードからトークンを取得する

```
$ curl https://api.dropbox.com/oauth2/token \
    -d code={1.で取得したコード} \
    -d grant_type=authorization_code \
    -d client_id={APP_KEY} \
    -d client_secret={APP_SECRET}
{
    "access_token": "xxxx",
    "token_type": "bearer",
    "expires_in": 14400,
    "refresh_token": "xxxxx",
    ...
}
```

3. refresh_token から access_token を取得する

```
$ curl -o - https://api.dropbox.com/oauth2/token \
    -d grant_type=refresh_token \
    -d refresh_token={2.で取得したrefresh_token} \
    -d client_id={APP_KEY} \
    -d client_secret={APP_SECRET}
{
    "access_token": "xxxx",
    "token_type": "bearer",
    "expires_in": 14400
}
```
