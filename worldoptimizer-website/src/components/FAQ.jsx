import React, { useState } from 'react';

const FAQ = () => {
    const [openIndex, setOpenIndex] = useState(null);

    const faqs = [
        {
            id: 1,
            question: 'World Optimizerは無料ですか?',
            answer: '現在、オープンソースプロジェクトとして無料で提供しています。将来的にプレミアム機能を追加する可能性があります。'
        },
        {
            id: 2,
            question: 'どのUnityバージョンに対応していますか?',
            answer: 'Unity 2021 LTS以降に対応しています。最新のLTSバージョンでの使用を推奨します。'
        },
        {
            id: 3,
            question: '最適化によってビジュアルが劣化することはありますか?',
            answer: 'World Optimizerは品質を維持しながら最適化を行うよう設計されていますが、大幅なファイルサイズ削減を行う場合、若干の品質低下が発生する可能性があります。バックアップ機能を使用して、いつでも元の状態に戻すことができます。'
        },
        {
            id: 4,
            question: 'VRChatワールド以外でも使用できますか?',
            answer: 'はい、Unityで作成されたあらゆるプロジェクトで使用できます。ゲーム、アプリ、XR体験など、様々な用途に対応しています。'
        },
        {
            id: 5,
            question: 'バックアップはどこに保存されますか?',
            answer: 'バックアップはプロジェクトフォルダ内の「WorldOptimizer_Backups」ディレクトリに保存されます。日時付きでバージョン管理されます。'
        }
    ];

    const toggleFAQ = (index) => {
        setOpenIndex(openIndex === index ? null : index);
    };

    return (
        <section className="faq">
            <div className="container">
                <h2 className="section-title">よくある質問</h2>
                <div className="faq-list">
                    {faqs.map((faq, index) => (
                        <div key={faq.id} className="faq-item">
                            <button
                                className={`faq-question ${openIndex === index ? 'active' : ''}`}
                                onClick={() => toggleFAQ(index)}
                                aria-expanded={openIndex === index}
                                aria-label={`質問: ${faq.question}`}
                            >
                                <span>{faq.question}</span>
                                <span className="faq-icon">{openIndex === index ? '−' : '+'}</span>
                            </button>
                            {openIndex === index && (
                                <div className="faq-answer">
                                    <p>{faq.answer}</p>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};

export default FAQ;
