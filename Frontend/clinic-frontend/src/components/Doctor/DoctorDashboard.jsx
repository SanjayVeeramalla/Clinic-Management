import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { doctorAPI } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';

const statusBadge = (status) => {
  const map = { Pending: 'badge-pending', Confirmed: 'badge-confirmed', Completed: 'badge-completed', Cancelled: 'badge-cancelled' };
  return <span className={`badge ${map[status] || ''}`}>{status}</span>;
};

const DoctorDashboard = () => {
  const { user } = useAuth();
  const [profile, setProfile]           = useState(null);
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading]           = useState(true);
  const [updating, setUpdating]         = useState(null);

  useEffect(() => {
    doctorAPI.getProfile()
      .then(async res => {
        const p = res.data.data;
        setProfile(p);
        const aptsRes = await doctorAPI.getAppointments(p.doctorId);
        setAppointments(aptsRes.data.data || []);
      })
      .catch(() => toast.error('Failed to load profile'))
      .finally(() => setLoading(false));
  }, []);

  const updateStatus = async (appointmentId, status) => {
    setUpdating(appointmentId);
    try {
      await doctorAPI.updateAppointmentStatus(appointmentId, { status });
      setAppointments(prev => prev.map(a =>
        a.appointmentId === appointmentId ? { ...a, status } : a
      ));
      toast.success(`Appointment marked as ${status}`);
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to update status');
    } finally {
      setUpdating(null);
    }
  };

  const todayStr = new Date().toISOString().split('T')[0];
  const todayApts = appointments.filter(a => a.appointmentDate?.startsWith(todayStr));
  const pending   = appointments.filter(a => a.status === 'Pending');
  const completed = appointments.filter(a => a.status === 'Completed');

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Dr. {user?.fullName}'s Dashboard</h1>
          {profile && (
            <p className="text-muted">{profile.specialization} — {profile.yearsOfExperience} yrs experience</p>
          )}
        </div>
        <Link to="/doctor/appointments" className="btn btn-primary">📋 All Appointments</Link>
      </div>

      <div className="stats-grid">
        <div className="stat-card stat-blue">
          <span className="stat-icon">🗓️</span>
          <h3>{todayApts.length}</h3>
          <p>Today's Appointments</p>
        </div>
        <div className="stat-card stat-yellow">
          <span className="stat-icon">⏳</span>
          <h3>{pending.length}</h3>
          <p>Pending</p>
        </div>
        <div className="stat-card stat-green">
          <span className="stat-icon">✅</span>
          <h3>{completed.length}</h3>
          <p>Completed</p>
        </div>
        <div className="stat-card stat-orange">
          <span className="stat-icon">💰</span>
          <h3>₹{profile?.consultationFee}</h3>
          <p>Consultation Fee</p>
        </div>
      </div>

      <div className="card">
        <div className="card-header">
          <h2>Today's Schedule</h2>
          <Link to="/doctor/schedule" className="btn btn-ghost btn-sm">Manage Schedule →</Link>
        </div>

        {todayApts.length === 0 ? (
          <div className="empty-state">
            <span className="empty-icon">📅</span>
            <h3>No appointments today</h3>
            <p>Enjoy your day off!</p>
          </div>
        ) : (
          todayApts.map(apt => (
            <div key={apt.appointmentId} className="appointment-card">
              <div className="apt-info">
                <h4>{apt.patientName}</h4>
                {apt.patientPhone && <p>📞 {apt.patientPhone}</p>}
                <p>⏰ {apt.appointmentTime?.slice(0,5)}</p>
                {apt.reasonForVisit && <p className="text-muted">📝 {apt.reasonForVisit}</p>}
              </div>
              <div className="apt-actions">
                {statusBadge(apt.status)}
                {apt.status === 'Pending' && (
                  <button className="btn btn-success btn-sm"
                    onClick={() => updateStatus(apt.appointmentId, 'Confirmed')}
                    disabled={updating === apt.appointmentId}>
                    Confirm
                  </button>
                )}
                {apt.status === 'Confirmed' && (
                  <>
                    <button className="btn btn-primary btn-sm"
                      onClick={() => updateStatus(apt.appointmentId, 'Completed')}
                      disabled={updating === apt.appointmentId}>
                      Complete
                    </button>
                    <Link to={`/doctor/prescription/${apt.appointmentId}`} className="btn btn-warning btn-sm">
                      💊 Prescribe
                    </Link>
                  </>
                )}
                {apt.status === 'Pending' && (
                  <button className="btn btn-ghost btn-sm"
                    onClick={() => updateStatus(apt.appointmentId, 'NoShow')}
                    disabled={updating === apt.appointmentId}>
                    No Show
                  </button>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default DoctorDashboard;