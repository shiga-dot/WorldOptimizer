import React from 'react';

const Support = () => {
    return (
        <div className="page-container support">
            <div className="support-content">
                <h1>サポート</h1>

                <div className="support-section">
                    <h2>お問い合わせ</h2>
                    <p>バグ報告や機能リクエストは、以下のフォームからお願いします。<br />
                        <small>※送信内容はあなたのメールアドレスに転送されます。</small></p>

                    {/* 
            TODO: 以下のURLの 'YOUR_FORM_ID' を、
            https://formspree.io/ で取得した実際のフォームIDに書き換えてください。
          */}
                    <form
                        className="contact-form"
                        action="https://formspree.io/f/myzrvdvn"
                        method="POST"
                    >
                        <div className="form-group">
                            <label>お名前</label>
                            <input type="text" name="name" placeholder="Your Name" required />
                        </div>
                        <div className="form-group">
                            <label>メールアドレス</label>
                            <input type="email" name="email" placeholder="email@example.com" required />
                        </div>
                        <div className="form-group">
                            <label>お問い合わせ内容</label>
                            <textarea name="message" rows="5" placeholder="詳細をご記入ください" required></textarea>
                        </div>
                        <button className="cta-button primary" type="submit">送信する</button>
                    </form>
                </div>

                <div className="support-section">
                    <h2>トラブルシューティング</h2>
                    <div className="troubleshoot-item">
                        <h3>Q. インストール時にエラーが発生する</h3>
                        <p>A. Unityのバージョンが2021.3 LTS以上であることを確認してください。</p>
                    </div>
                    <div className="troubleshoot-item">
                        <h3>Q. 最適化後にマテリアルがピンクになる</h3>
                        <p>A. シェーダーの互換性設定を確認し、「Restore Backup」から復元を試してください。</p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Support;
