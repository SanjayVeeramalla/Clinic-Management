import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { adminAPI } from '../../services/api';
import toast from 'react-hot-toast';

const AdminDashboard = () => {
  const [stats, setStats]   = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminAPI.getDashboard()
      .then(res => setStats(res.data.data))
      .catch(() => toast.error('Failed to load dashboard'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  const cards = [
    { label: 'Total Patients',         value: stats?.totalPatients,       color: 'blue',   icon: '👥', link: '/admin/patients' },
    { label: 'Total Doctors',          value: stats?.totalDoctors,        color: 'green',  icon: '👨‍⚕️', link: '/admin/doctors' },
    { label: 'Total Appointments',     value: stats?.totalAppointments,   color: 'purple', icon: '📅', link: null },
    { label: "Today's Appointments",   value: stats?.todayAppointments,   color: 'orange', icon: '🗓️', link: null },
    { label: 'Pending Appointments',   value: stats?.pendingAppointments, color: 'yellow', icon: '⏳', link: null },
    { label: 'Completed Appointments', value: stats?.completedAppointments, color: 'teal', icon: '✅', link: null },
  ];

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Admin Dashboard</h1>
          <p className="text-muted">Clinic Management System Overview</p>
        </div>
        <Link to="/admin/reports" className="btn btn-primary">📊 View Reports</Link>
      </div>

      <div className="stats-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))' }}>
        {cards.map(card => (
          <div key={card.label} className={`stat-card stat-${card.color}`}
            style={card.link ? { cursor: 'pointer' } : {}}
            onClick={() => card.link && window.location.assign(card.link)}>
            <span className="stat-icon">{card.icon}</span>
            <h3>{card.value ?? '—'}</h3>
            <p>{card.label}</p>
          </div>
        ))}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
        <div className="card">
          <div className="card-header"><h2>Quick Actions</h2></div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            <Link to="/admin/doctors" className="btn btn-outline btn-full">👨‍⚕️ Manage Doctors</Link>
            <Link to="/admin/patients" className="btn btn-outline btn-full">👥 Manage Patients</Link>
            <Link to="/admin/reports" className="btn btn-outline btn-full">📊 View Reports</Link>
          </div>
        </div>

        <div className="card">
          <div className="card-header"><h2>System Info</h2></div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', fontSize: '0.9rem' }}>
            {[
              ['Platform', 'Clinic Management System v1.0'],
              ['Backend',  'ASP.NET Core 8 Web API'],
              ['Database', 'SQL Server + Stored Procedures'],
              ['Auth',     'JWT + Refresh Tokens'],
              ['Frontend', 'React 18 + React Router'],
            ].map(([k, v]) => (
              <div key={k} style={{ display: 'flex', gap: '1rem' }}>
                <span style={{ width: '90px', fontWeight: 600, color: '#475569' }}>{k}</span>
                <span style={{ color: '#64748b' }}>{v}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;