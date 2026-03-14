import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';

const navLinks = {
  Patient: [
    { to: '/patient/dashboard',        label: '🏠 Dashboard' },
    { to: '/patient/book-appointment', label: '📅 Book' },
    { to: '/patient/appointments',     label: '📋 Appointments' },
    { to: '/patient/profile',          label: '👤 Profile' },
  ],
  Doctor: [
    { to: '/doctor/dashboard',    label: '🏠 Dashboard' },
    { to: '/doctor/appointments', label: '📋 Appointments' },
    { to: '/doctor/schedule',     label: '🗓️ Schedule' },
  ],
  Admin: [
    { to: '/admin/dashboard', label: '🏠 Dashboard' },
    { to: '/admin/doctors',   label: '👨‍⚕️ Doctors' },
    { to: '/admin/patients',  label: '👥 Patients' },
    { to: '/admin/reports',   label: '📊 Reports' },
  ],
};

const Navbar = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    toast.success('Logged out successfully');
    navigate('/login');
  };

  const links = user ? (navLinks[user.role] || []) : [];

  return (
    <nav className="navbar">
      <div className="navbar-brand">🏥 ClinicMS</div>

      <div className="navbar-links">
        {links.map(link => (
          <NavLink
            key={link.to}
            to={link.to}
            className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
          >
            {link.label}
          </NavLink>
        ))}
      </div>

      {user && (
        <div className="navbar-user">
          <span className="text-muted" style={{ color: 'rgba(255,255,255,0.8)', fontSize: '0.875rem' }}>
            {user.fullName}
          </span>
          <span className="user-badge">{user.role}</span>
          <button className="btn-logout" onClick={handleLogout}>Logout</button>
        </div>
      )}
    </nav>
  );
};

export default Navbar;