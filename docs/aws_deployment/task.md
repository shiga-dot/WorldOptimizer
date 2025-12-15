# AWS Lightsail Deployment Task List

- [x] Build project locally
- [/] Prepare Lightsail Instance (Option B Selected)
    - [x] Login to Lightsail Console
    - [x] Create Instance (Ubuntu OS Blueprint - User: `ubuntu`)
    - [x] Attach Static IP (IP: `13.230.53.136`)
    - [x] Download SSH Key Pair (Key: `LightsailDefaultKey-ap-northeast-1 (1).pem`)

- [ ] (Option B Only) Install Requirements on Ubuntu
    - [ ] Update OS: `sudo apt update && sudo apt upgrade -y`
    - [ ] Install Nginx: `sudo apt install nginx -y`

    - [ ] Enable Firewall: `sudo ufw allow 'Nginx Full'`
    - [ ] Verify Nginx is running: `systemctl status nginx`

- [ ] Deploy Files
    - [ ] Locate `.pem` key file locally
    - [ ] Upload `dist` content via SCP
        - Target path (Ubuntu): `/var/www/html` (Requires permissions check)
        - Target path (Bitnami): `/home/bitnami/htdocs`
    
- [ ] Configure Nginx
    - [ ] Configuration Path:
        - Ubuntu: `/etc/nginx/sites-available/default`
        - Bitnami: `/opt/bitnami/nginx/conf/server_blocks/my-app.conf`
    - [ ] Setup SPA routing (`try_files $uri /index.html`)
    - [ ] Restart Nginx

- [ ] Verification
    - [ ] Access site via Static IP
