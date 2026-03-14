import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { doctorAPI } from '../../services/api';
import toast from 'react-hot-toast';

const statusBadge = (status) => {
  const map = { Pending: 'badge-pending', Confirmed: 'badge-confirmed', Completed: 'badge-completed', Cancelled: 'badge-cancelled', NoShow: 'badge-noshow' };
  return <span className={`badge ${map[status] || ''}`}>{status}</span>;
};

const DoctorAppointments = () => {
  const [profile, setProfile]           = useState(null);
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading]           = useState(true);
  const [activeTab, setActiveTab]       = useState('all');
  const [updating, setUpdating]         = useState(null);

  useEffect(() => {
    doctorAPI.getProfile()
      .then(async res => {
        const p = res.data.data;
        setProfile(p);
        const aptsRes = await doctorAPI.getAppointments(p.doctorId);
        setAppointments(aptsRes.data.data || []);
      })
      .catch(() => toast.error('Failed to load appointments'))
      .finally(() => setLoading(false));
  }, []);

  const updateStatus = async (appointmentId, status) => {
    setUpdating(appointmentId);
    try {
      await doctorAPI.updateAppointmentStatus(appointmentId, { status });
      setAppointments(prev => prev.map(a =>
        a.appointmentId === appointmentId ? { ...a, status } : a
      ));
      toast.success(`Marked as ${status}`);
    } catch (err) {
      toast.error(err.response?.data?.message || 'Update failed');
    } finally {
      setUpdating(null);
    }
  };

  const tabs = [
    { key: 'all',       label: `All (${appointments.length})` },
    { key: 'Pending',   label: `Pending (${appointments.filter(a => a.status === 'Pending').length})` },
    { key: 'Confirmed', label: `Confirmed (${appointments.filter(a => a.status === 'Confirmed').length})` },
    { key: 'Completed', label: `Completed (${appointments.filter(a => a.status === 'Completed').length})` },
  ];

  const filtered = activeTab === 'all' ? appointments : appointments.filter(a => a.status === activeTab);

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>My Appointments</h1>
          <p className="text-muted">View and manage patient appointments</p>
        </div>
      </div>

      <div className="tabs">
        {tabs.map(t => (
          <button key={t.key} className={`tab ${activeTab === t.key ? 'active' : ''}`} onClick={() => setActiveTab(t.key)}>
            {t.label}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="empty-state"><span className="empty-icon">📭</span><h3>No appointments</h3></div>
      ) : (
        filtered.map(apt => (
          <div key={apt.appointmentId} className="appointment-card">
            <div className="apt-info">
              <h4>{apt.patientName}</h4>
              {apt.patientPhone && <p>📞 {apt.patientPhone}</p>}
              <p>
                📆 {new Date(apt.appointmentDate).toLocaleDateString('en-IN', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' })}
                &nbsp;at ⏰ {apt.appointmentTime?.slice(0,5)}
              </p>
              {apt.reasonForVisit && <p className="text-muted">📝 {apt.reasonForVisit}</p>}
              {apt.notes && <p className="text-muted">🗒️ {apt.notes}</p>}
            </div>
            <div className="apt-actions">
              {statusBadge(apt.status)}
              {apt.status === 'Pending' && (
                <>
                  <button className="btn btn-success btn-sm"
                    onClick={() => updateStatus(apt.appointmentId, 'Confirmed')}
                    disabled={updating === apt.appointmentId}>Confirm</button>
                  <button className="btn btn-ghost btn-sm"
                    onClick={() => updateStatus(apt.appointmentId, 'NoShow')}
                    disabled={updating === apt.appointmentId}>No Show</button>
                </>
              )}
              {apt.status === 'Confirmed' && (
                <>
                  <button className="btn btn-primary btn-sm"
                    onClick={() => updateStatus(apt.appointmentId, 'Completed')}
                    disabled={updating === apt.appointmentId}>Complete</button>
                  <Link to={`/doctor/prescription/${apt.appointmentId}`} className="btn btn-warning btn-sm">
                    💊 Prescribe
                  </Link>
                </>
              )}
              {apt.status === 'Completed' && !apt.prescription && (
                <Link to={`/doctor/prescription/${apt.appointmentId}`} className="btn btn-outline btn-sm">
                  Add Rx
                </Link>
              )}
            </div>
          </div>
        ))
      )}
    </div>
  );
};

export default DoctorAppointments;