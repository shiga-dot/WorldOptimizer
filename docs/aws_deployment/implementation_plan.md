# AWS Lightsail デプロイ手順 (Ubuntu)

## 接続情報
- **IP**: `13.230.53.136`
- **User**: `ubuntu`
- **Key**: `C:\Users\miiku\Documents\新しいフォルダー\LightsailDefaultKey-ap-northeast-1 (1).pem`

## Step 1: ファイルのアップロード (ローカル PowerShell)

まず、ビルドしたファイルをサーバーのホームディレクトリへアップロードします。
(直接 `/var/www` には権限がなく送れないため、一度ホームに送ります)

```powershell
# プロジェクトディレクトリへ移動
cd c:\Users\miiku\.gemini\antigravity\playground\stellar-ionosphere\worldoptimizer-website

# SCPコマンド実行 (distフォルダごと転送)
scp -i "C:\Users\miiku\Documents\新しいフォルダー\LightsailDefaultKey-ap-northeast-1 (1).pem" -r dist ubuntu@13.230.53.136:/home/ubuntu
```

## Step 2: サーバーセットアップ (SSH接続後)

SSHでサーバーに入り、Nginxのインストールとファイルの配置を行います。

### 2-1. SSH接続
```powershell
ssh -i "C:\Users\miiku\Documents\新しいフォルダー\LightsailDefaultKey-ap-northeast-1 (1).pem" ubuntu@13.230.53.136
```

### 2-2. サーバー内でのコマンド (一括実行用)
サーバーに入ったら、以下のコマンドを1行ずつ、またはまとめて実行してください。

```bash
# システム更新とNginxインストール
sudo apt update
sudo apt install nginx -y

# ファイアウォール許可
sudo ufw allow 'Nginx Full'

# デフォルトのhtmlを削除し、アップロードしたファイルで上書き
sudo rm -rf /var/www/html/*
sudo cp -r /home/ubuntu/dist/* /var/www/html/

# 権限の修正
sudo chown -R www-data:www-data /var/www/html
sudo chmod -R 755 /var/www/html

# Nginx設定 (SPA対応 - 404防止)
# 以下のコマンドで設定ファイルを上書きします
sudo bash -c 'cat > /etc/nginx/sites-available/default <<EOF
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    root /var/www/html;
    index index.html index.htm index.nginx-debian.html;

    server_name _;

    location / {
        try_files \$uri \$uri/ /index.html;
    }
}
EOF'

# Nginx再起動
sudo systemctl restart nginx
```

## Step 3: 確認
ブラウザで `http://13.230.53.136` にアクセスしてページが表示されるか確認します。
