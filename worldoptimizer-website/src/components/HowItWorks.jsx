import React from 'react';

const HowItWorks = () => {
    const steps = [
        {
            id: 1,
            number: '01',
            title: 'インポート',
            description: 'World OptimizerをUnityプロジェクトにインポート'
        },
        {
            id: 2,
            number: '02',
            title: '設定',
            description: 'メニューから「World Optimizer」を開き、最適化オプションを選択'
        },
        {
            id: 3,
            number: '03',
            title: '実行',
            description: '「最適化実行」ボタンをクリックして完了を待つ'
        }
    ];

    return (
        <section className="how-it-works">
            <div className="container">
                <h2 className="section-title">使い方</h2>
                <div className="steps-container">
                    {steps.map((step, index) => (
                        <div key={step.id} className="step">
                            <div className="step-number">{step.number}</div>
                            <h3 className="step-title">{step.title}</h3>
                            <p className="step-description">{step.description}</p>
                            {index < steps.length - 1 && <div className="step-arrow">→</div>}
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};

export default HowItWorks;
