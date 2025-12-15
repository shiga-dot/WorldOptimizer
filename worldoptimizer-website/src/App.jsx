import React, { useState, useEffect } from 'react';
import Hero from './components/Hero';
import Features from './components/Features';
import Functions from './components/Functions';
import HowItWorks from './components/HowItWorks';
import UseCases from './components/UseCases';
import FAQ from './components/FAQ';
import Footer from './components/Footer';
import Documentation from './components/Documentation';
import Support from './components/Support';
import ReleaseNotes from './components/ReleaseNotes';
import './App.css';

function App() {
    const [currentPage, setCurrentPage] = useState('home');

    useEffect(() => {
        const handleHashChange = () => {
            const hash = window.location.hash.replace('#', '');
            if (['documentation', 'support', 'releases'].includes(hash)) {
                setCurrentPage(hash);
                window.scrollTo(0, 0);
            } else {
                setCurrentPage('home');
            }
        };

        handleHashChange();
        window.addEventListener('hashchange', handleHashChange);
        return () => window.removeEventListener('hashchange', handleHashChange);
    }, []);

    const renderContent = () => {
        switch (currentPage) {
            case 'documentation':
                return <Documentation />;
            case 'support':
                return <Support />;
            case 'releases':
                return <ReleaseNotes />;
            default:
                return (
                    <>
                        <Hero />
                        <Features />
                        <Functions />
                        <HowItWorks />
                        <UseCases />
                        <FAQ />
                    </>
                );
        }
    };

    return (
        <div className="App">
            {currentPage !== 'home' && (
                <nav className="navbar">
                    <div className="container nav-content">
                        <a href="#" className="nav-logo">World Optimizer</a>
                        <a href="#" className="nav-back">← トップへ戻る</a>
                    </div>
                </nav>
            )}

            {renderContent()}

            <Footer />
        </div>
    );
}

export default App;
