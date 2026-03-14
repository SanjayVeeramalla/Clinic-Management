import React, { useEffect, useState } from 'react';
import { patientAPI } from '../../services/api';
import toast from 'react-hot-toast';

const statusBadge = (status) => {
  const map = { Pending: 'badge-pending', Confirmed: 'badge-confirmed', Completed: 'badge-completed', Cancelled: 'badge-cancelled', NoShow: 'badge-noshow' };
  return <span className={`badge ${map[status] || ''}`}>{status}</span>;
};

const MyAppointments = () => {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('all');
  const [cancelModal, setCancelModal] = useState(null);
  const [cancelReason, setCancelReason] = useState('');
  const [cancelling, setCancelling] = useState(false);

  const loadAppointments = () => {
    setLoading(true);
    patientAPI.getAppointments()
      .then(res => setAppointments(res.data.data || []))
      .catch(() => toast.error('Failed to load appointments'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadAppointments(); }, []);

  const filtered = appointments.filter(a => {
    if (activeTab === 'upcoming')  return ['Pending','Confirmed'].includes(a.status);
    if (activeTab === 'completed') return a.status === 'Completed';
    if (activeTab === 'cancelled') return a.status === 'Cancelled';
    return true;
  });

  const handleCancel = async () => {
    if (!cancelReason.trim()) { toast.error('Please provide a reason'); return; }
    setCancelling(true);
    try {
      await patientAPI.cancelAppointment(cancelModal.appointmentId, { cancellationReason: cancelReason });
      toast.success('Appointment cancelled');
      setCancelModal(null);
      setCancelReason('');
      loadAppointments();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Cancellation failed');
    } finally {
      setCancelling(false);
    }
  };

  const tabs = [
    { key: 'all',       label: `All (${appointments.length})` },
    { key: 'upcoming',  label: `Upcoming (${appointments.filter(a => ['Pending','Confirmed'].includes(a.status)).length})` },
    { key: 'completed', label: `Completed (${appointments.filter(a => a.status === 'Completed').length})` },
    { key: 'cancelled', label: `Cancelled (${appointments.filter(a => a.status === 'Cancelled').length})` },
  ];

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>My Appointments</h1>
          <p className="text-muted">View and manage your appointments</p>
        </div>
      </div>

      <div className="tabs">
        {tabs.map(tab => (
          <button
            key={tab.key}
            className={`tab ${activeTab === tab.key ? 'active' : ''}`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="empty-state">
          <span className="empty-icon">📭</span>
          <h3>No appointments found</h3>
        </div>
      ) : (
        filtered.map(apt => (
          <div key={apt.appointmentId} className="appointment-card">
            <div className="apt-info">
              <h4>Dr. {apt.doctorName}</h4>
              <p>🏥 {apt.specialization}</p>
              <p>
                📆 {new Date(apt.appointmentDate).toLocaleDateString('en-IN', {
                  weekday: 'short', year: 'numeric', month: 'short', day: 'numeric'
                })} at ⏰ {apt.appointmentTime?.slice(0,5)}
              </p>
              {apt.reasonForVisit && <p className="text-muted">📝 {apt.reasonForVisit}</p>}
              {apt.cancellationReason && <p className="text-danger">❌ {apt.cancellationReason}</p>}

              {/* Prescription block */}
              {apt.prescription && (
                <div className="prescription-box" style={{ marginTop: '0.75rem' }}>
                  <h5>💊 Prescription</h5>
                  {apt.prescription.diagnosis   && <p><strong>Diagnosis:</strong> {apt.prescription.diagnosis}</p>}
                  {apt.prescription.medications && <p><strong>Medications:</strong> {apt.prescription.medications}</p>}
                  {apt.prescription.instructions && <p><strong>Instructions:</strong> {apt.prescription.instructions}</p>}
                  {apt.prescription.followUpDate && <p><strong>Follow-up:</strong> {new Date(apt.prescription.followUpDate).toLocaleDateString('en-IN')}</p>}
                </div>
              )}
            </div>

            <div className="apt-actions">
              {statusBadge(apt.status)}
              {['Pending', 'Confirmed'].includes(apt.status) && (
                <button
                  className="btn btn-danger btn-sm"
                  onClick={() => { setCancelModal(apt); setCancelReason(''); }}
                >
                  Cancel
                </button>
              )}
            </div>
          </div>
        ))
      )}

      {/* Cancel Modal */}
      {cancelModal && (
        <div className="modal-overlay" onClick={() => setCancelModal(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Cancel Appointment</h3>
              <button className="modal-close" onClick={() => setCancelModal(null)}>×</button>
            </div>
            <p className="text-muted mb-2">
              Cancel appointment with Dr. {cancelModal.doctorName} on{' '}
              {new Date(cancelModal.appointmentDate).toLocaleDateString('en-IN')}?
            </p>
            <div className="form-group">
              <label>Reason for cancellation *</label>
              <textarea
                value={cancelReason}
                onChange={e => setCancelReason(e.target.value)}
                rows={3}
                placeholder="Please provide a reason..."
              />
            </div>
            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={() => setCancelModal(null)}>Keep</button>
              <button className="btn btn-danger" onClick={handleCancel} disabled={cancelling}>
                {cancelling ? 'Cancelling...' : 'Yes, Cancel'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyAppointments;