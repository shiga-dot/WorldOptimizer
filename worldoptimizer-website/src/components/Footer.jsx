import React from 'react';

const Footer = () => {
    return (
        <footer className="footer">
            <div className="container">
                <div className="footer-content">
                    <div className="footer-section">
                        <h3 className="footer-title">World Optimizer</h3>
                        <p className="footer-description">
                            Unityワールドを最適化し、<br />
                            パフォーマンスを最大化
                        </p>
                    </div>

                    <div className="footer-section">
                        <h4 className="footer-heading">リンク</h4>
                        <ul className="footer-links">
                            <li><a href="#documentation">ドキュメント</a></li>
                            <li><a href="https://github.com" target="_blank" rel="noopener noreferrer">GitHub</a></li>
                            <li><a href="#releases">リリースノート</a></li>
                            <li><a href="#support">サポート</a></li>
                        </ul>
                    </div>
                </div>

                <div className="footer-bottom">
                    <p className="copyright">
                        &copy; 2025 World Optimizer. All rights reserved.
                    </p>
                </div>
            </div>
        </footer>
    );
};

export default Footer;
