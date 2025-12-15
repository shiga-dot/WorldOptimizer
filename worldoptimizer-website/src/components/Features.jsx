import React from 'react';

const Features = () => {
    const features = [
        {
            id: 1,
            icon: '⚡',
            title: 'ワンクリック最適化',
            description: '複雑な設定不要。ボタン一つで総合的な最適化が完了'
        },
        {
            id: 2,
            icon: '🔍',
            title: '詳細な分析',
            description: 'ワールドの問題点を自動検出し、改善提案を表示'
        },
        {
            id: 3,
            icon: '💾',
            title: '安全なバックアップ',
            description: '最適化前に自動でバックアップを作成。いつでも復元可能'
        },
        {
            id: 4,
            icon: '📊',
            title: 'パフォーマンス向上',
            description: 'FPS改善、ロード時間短縮、メモリ使用量削減を実現'
        },
        {
            id: 5,
            icon: '🎨',
            title: '品質維持',
            description: 'ビジュアル品質を保ちながら最適化を実行'
        }
    ];

    return (
        <section className="features">
            <div className="container">
                <h2 className="section-title">主な特徴</h2>
                <div className="features-grid">
                    {features.map(feature => (
                        <div key={feature.id} className="feature-card">
                            <div className="feature-icon">{feature.icon}</div>
                            <h3 className="feature-title">{feature.title}</h3>
                            <p className="feature-description">{feature.description}</p>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};

export default Features;
