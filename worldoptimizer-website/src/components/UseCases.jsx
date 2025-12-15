import React from 'react';

const UseCases = () => {
    const useCases = [
        {
            id: 1,
            title: 'VRChatワールドの最適化',
            description: 'アップロード前にワールドを最適化し、訪問者に快適な体験を提供。テクスチャやメッシュを自動で調整し、パフォーマンスを大幅に向上。',
            benefit: 'FPS向上、ロード時間短縮'
        },
        {
            id: 2,
            title: 'モバイルゲーム開発',
            description: 'リソース制限の厳しいモバイル環境向けに、アセットを最適化。メモリ使用量を削減し、幅広いデバイスでの動作を実現。',
            benefit: 'メモリ削減、互換性向上'
        },
        {
            id: 3,
            title: 'プロトタイプの軽量化',
            description: '開発中のプロトタイプを軽量化し、テストイテレーションを高速化。チーム内での共有やテストプレイがスムーズに。',
            benefit: '開発効率向上、共有容易化'
        }
    ];

    return (
        <section className="use-cases">
            <div className="container">
                <h2 className="section-title">利用シーン</h2>
                <div className="use-cases-grid">
                    {useCases.map(useCase => (
                        <div key={useCase.id} className="use-case-card">
                            <h3 className="use-case-title">{useCase.title}</h3>
                            <p className="use-case-description">{useCase.description}</p>
                            <div className="use-case-benefit">
                                <span className="benefit-label">効果:</span> {useCase.benefit}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};

export default UseCases;
