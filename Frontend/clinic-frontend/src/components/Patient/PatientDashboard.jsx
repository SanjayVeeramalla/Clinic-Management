import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { patientAPI } from '../../services/api';
import { useAuth } from '../../context/AuthContext';

const statusBadge = (status) => {
  const map = {
    Pending:   'badge-pending',
    Confirmed: 'badge-confirmed',
    Completed: 'badge-completed',
    Cancelled: 'badge-cancelled',
    NoShow:    'badge-noshow',
  };
  return <span className={`badge ${map[status] || ''}`}>{status}</span>;
};

const PatientDashboard = () => {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    patientAPI.getAppointments()
      .then(res => setAppointments(res.data.data || []))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const upcoming = appointments.filter(a =>
    ['Pending', 'Confirmed'].includes(a.status)
  );
  const completed = appointments.filter(a => a.status === 'Completed');
  const cancelled = appointments.filter(a => a.status === 'Cancelled');

  if (loading) {
    return <div className="loading"><div className="spinner" /><span>Loading dashboard...</span></div>;
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Welcome, {user?.fullName}! 👋</h1>
          <p className="text-muted">Manage your health appointments</p>
        </div>
        <Link to="/patient/book-appointment" className="btn btn-primary">
          📅 Book Appointment
        </Link>
      </div>

      {/* Stats */}
      <div className="stats-grid">
        <div className="stat-card stat-blue">
          <span className="stat-icon">📅</span>
          <h3>{appointments.length}</h3>
          <p>Total Appointments</p>
        </div>
        <div className="stat-card stat-yellow">
          <span className="stat-icon">⏳</span>
          <h3>{upcoming.length}</h3>
          <p>Upcoming</p>
        </div>
        <div className="stat-card stat-green">
          <span className="stat-icon">✅</span>
          <h3>{completed.length}</h3>
          <p>Completed</p>
        </div>
        <div className="stat-card stat-red">
          <span className="stat-icon">❌</span>
          <h3>{cancelled.length}</h3>
          <p>Cancelled</p>
        </div>
      </div>

      {/* Upcoming appointments */}
      <div className="card">
        <div className="card-header">
          <h2>Upcoming Appointments</h2>
          <Link to="/patient/appointments" className="btn btn-ghost btn-sm">View All →</Link>
        </div>

        {upcoming.length === 0 ? (
          <div className="empty-state">
            <span className="empty-icon">📭</span>
            <h3>No upcoming appointments</h3>
            <p>Book an appointment with one of our doctors</p>
            <Link to="/patient/book-appointment" className="btn btn-primary">Book Now</Link>
          </div>
        ) : (
          upcoming.slice(0, 5).map(apt => (
            <div key={apt.appointmentId} className="appointment-card">
              <div className="apt-info">
                <h4>Dr. {apt.doctorName}</h4>
                <p>🏥 {apt.specialization}</p>
                <p>
                  📆 {new Date(apt.appointmentDate).toLocaleDateString('en-IN', {
                    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
                  })}
                  &nbsp;at&nbsp;
                  ⏰ {apt.appointmentTime?.slice(0, 5)}
                </p>
                {apt.reasonForVisit && <p className="text-muted">📝 {apt.reasonForVisit}</p>}
              </div>
              <div className="apt-actions">
                {statusBadge(apt.status)}
              </div>
            </div>
          ))
        )}
      </div>

      {/* Recent completed */}
      {completed.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h2>Recent Visits</h2>
          </div>
          {completed.slice(0, 3).map(apt => (
            <div key={apt.appointmentId} className="appointment-card">
              <div className="apt-info">
                <h4>Dr. {apt.doctorName}</h4>
                <p>{apt.specialization} — {new Date(apt.appointmentDate).toLocaleDateString('en-IN')}</p>
                {apt.prescription?.diagnosis && (
                  <p className="text-muted">Diagnosis: {apt.prescription.diagnosis}</p>
                )}
              </div>
              {statusBadge(apt.status)}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default PatientDashboard;