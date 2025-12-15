import React from 'react';

const Hero = () => {
    // 背景用のコードスニペット - 量を増やして画面全体を確実に埋める
    const codeSnippet = `using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace WorldOptimizer
{
    public class OptimizerWindow : EditorWindow
    {
        [MenuItem("Tools/World Optimizer")]
        public static void ShowWindow()
        {
            var window = GetWindow<OptimizerWindow>("World Optimizer");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Optimization Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Analyze Scene"))
            {
                SceneAnalyzer.Analyze();
            }

            if (GUILayout.Button("Start Optimization"))
            {
                OptimizeTextures();
                CombineMeshes();
                CleanValidations();
            }
        }

        private void OptimizeTextures()
        {
            foreach (var tex in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                TextureCompressor.Compress(tex);
                Debug.Log($"Compressed: {tex.name}");
            }
        }

        private void CombineMeshes()
        {
            var meshCombiner = new MeshCombiner();
            meshCombiner.CombineStaticMeshes();
            // Optimized draw calls significantly
        }

        // Performance Check
        // Memory Optimization
        // Auto Backup System
        // Lighting Data Compression
        // Audio Clip Optimization
        // Shader Variant Stripping
        
        void Update() {
            // Realtime Monitoring
            if (isOptimizing) {
                MonitorPerformance();
            }
        }
    }
}
`.repeat(6);

    return (
        <section className="hero">
            <div className="hero-code-background">
                <pre>
                    <code>{codeSnippet}</code>
                </pre>
            </div>
            <div className="hero-overlay"></div>
            <div className="hero-content">
                <h1 className="hero-title">
                    World <span className="text-glow">Optimizer</span>
                </h1>
                <p className="hero-subtitle">
                    Unlock Maximum Performance
                </p>
                <p className="hero-description">
                    究極のUnityワールド最適化ツール。<br />
                    ワンクリックで、あなたのワールドを限界まで軽量化。
                </p>
                <div className="hero-buttons">
                    <button className="cta-button primary" onClick={() => window.scrollTo({ top: document.querySelector('.functions').offsetTop, behavior: 'smooth' })}>
                        機能を見る
                    </button>
                    <button className="cta-button secondary">
                        GitHubで見る
                    </button>
                </div>
            </div>
        </section>
    );
};

export default Hero;
