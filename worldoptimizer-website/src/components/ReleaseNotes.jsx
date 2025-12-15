import React from 'react';

const ReleaseNotes = () => {
    const releases = [
        {
            version: 'v1.2.0',
            date: '2025.12.01',
            title: 'メッシュ最適化機能の強化',
            changes: [
                'LOD生成アルゴリズムの改善',
                'バックアップ機能の高速化',
                'UIのダークモード対応'
            ]
        },
        {
            version: 'v1.1.5',
            date: '2025.11.15',
            title: 'バグ修正アップデート',
            changes: [
                'テクスチャ圧縮時のメモリリークを修正',
                '一部のシェーダーで発生していた表示崩れを修正'
            ]
        },
        {
            version: 'v1.0.0',
            date: '2025.10.01',
            title: '初回リリース',
            changes: [
                '基本機能の実装',
                'ワンクリック最適化機能',
                'シーン分析ツール'
            ]
        }
    ];

    return (
        <div className="page-container releases">
            <h1>リリースノート</h1>
            <div className="timeline">
                {releases.map((release, index) => (
                    <div key={index} className="release-item">
                        <div className="release-header">
                            <span className="release-version">{release.version}</span>
                            <span className="release-date">{release.date}</span>
                        </div>
                        <h2 className="release-title">{release.title}</h2>
                        <ul className="release-changes">
                            {release.changes.map((change, i) => (
                                <li key={i}>{change}</li>
                            ))}
                        </ul>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default ReleaseNotes;
