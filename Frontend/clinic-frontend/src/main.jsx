// Vite entry point — equivalent to CRA's index.js
// Key difference: file is named main.jsx (not index.js)
// and is referenced directly from index.html via <script type="module">

import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App.jsx';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);