import React, { useEffect, useState } from 'react';
import { adminAPI, specializationsAPI } from '../../services/api';
import toast from 'react-hot-toast';

const ManageDoctors = () => {
  const [doctors, setDoctors]                   = useState([]);
  const [specializations, setSpecializations]   = useState([]);
  const [loading, setLoading]                   = useState(true);
  const [showModal, setShowModal]               = useState(false);
  const [deactivating, setDeactivating]         = useState(null);
  const [saving, setSaving]                     = useState(false);

  // All fields for creating a full doctor account in one step
  const emptyForm = {
    fullName: '', email: '', password: '', confirmPassword: '',
    phone: '', specializationId: '', licenseNumber: '',
    yearsOfExperience: 0, consultationFee: 500,
  };
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState('');

  const load = () => {
    setLoading(true);
    Promise.all([adminAPI.getDoctors(), specializationsAPI.getAll()])
      .then(([docRes, specRes]) => {
        setDoctors(docRes.data.data || []);
        setSpecializations(specRes.data.data || []);
      })
      .catch(() => toast.error('Failed to load data'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleChange = (e) => {
    setFormError('');
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const openModal = () => {
    setForm(emptyForm);
    setFormError('');
    setShowModal(true);
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setFormError('');

    if (form.password !== form.confirmPassword) {
      setFormError('Passwords do not match');
      return;
    }
    if (form.password.length < 6) {
      setFormError('Password must be at least 6 characters');
      return;
    }

    setSaving(true);
    try {
      const { confirmPassword, ...payload } = form;
      await adminAPI.createDoctorAccount({
        ...payload,
        specializationId:  parseInt(form.specializationId),
        yearsOfExperience: parseInt(form.yearsOfExperience),
        consultationFee:   parseFloat(form.consultationFee),
      });
      toast.success(`Doctor account created for ${form.fullName}!`);
      setShowModal(false);
      load();
    } catch (err) {
      setFormError(err.response?.data?.message || 'Failed to create doctor account');
    } finally {
      setSaving(false);
    }
  };

  const handleDeactivate = async (userId, name) => {
    if (!window.confirm(`Deactivate ${name}? This will cancel their upcoming appointments.`)) return;
    setDeactivating(userId);
    try {
      await adminAPI.deactivateUser(userId);
      toast.success(`${name} deactivated`);
      load();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Deactivation failed');
    } finally {
      setDeactivating(null);
    }
  };

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Manage Doctors</h1>
          <p className="text-muted">{doctors.length} doctors registered</p>
        </div>
        <button className="btn btn-primary" onClick={openModal}>
          ➕ Create Doctor Account
        </button>
      </div>

      {doctors.length === 0 ? (
        <div className="empty-state">
          <span className="empty-icon">👨‍⚕️</span>
          <h3>No doctors yet</h3>
          <p>Create the first doctor account using the button above</p>
        </div>
      ) : (
        <div className="doctors-grid">
          {doctors.map(d => (
            <div key={d.doctorId} className="doctor-card">
              <h4>Dr. {d.fullName}</h4>
              <p className="spec">{d.specialization}</p>
              <p className="detail">📧 {d.email}</p>
              {d.phone && <p className="detail">📞 {d.phone}</p>}
              <p className="detail">🪪 {d.licenseNumber}</p>
              <p className="detail">⭐ {d.yearsOfExperience} yrs experience</p>
              <p className="fee">₹{d.consultationFee}</p>
              <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem', alignItems: 'center' }}>
                <span className={`badge ${d.isAvailable ? 'badge-confirmed' : 'badge-cancelled'}`}>
                  {d.isAvailable ? 'Available' : 'Unavailable'}
                </span>
                <button
                  className="btn btn-danger btn-sm"
                  style={{ marginLeft: 'auto' }}
                  onClick={() => handleDeactivate(d.userId, d.fullName)}
                  disabled={deactivating === d.userId}
                >
                  {deactivating === d.userId ? '...' : 'Deactivate'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create Doctor Account Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" style={{ maxWidth: '580px' }} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>👨‍⚕️ Create Doctor Account</h3>
              <button className="modal-close" onClick={() => setShowModal(false)}>×</button>
            </div>

            <p className="text-muted" style={{ marginBottom: '1rem', fontSize: '0.875rem' }}>
              Creates a login account and doctor profile in one step. The doctor can log in immediately using the email and password you set.
            </p>

            {formError && <div className="alert alert-error" style={{ marginBottom: '1rem' }}>⚠️ {formError}</div>}

            <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>

              {/* Personal info */}
              <div style={{ borderBottom: '1px solid #f1f5f9', paddingBottom: '0.75rem', marginBottom: '0.25rem' }}>
                <p style={{ fontSize: '0.8rem', fontWeight: 700, color: '#64748b', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.75rem' }}>Account Details</p>
                <div className="form-group" style={{ marginBottom: '0.75rem' }}>
                  <label>Full Name *</label>
                  <input name="fullName" value={form.fullName} onChange={handleChange}
                    placeholder="Dr. Jane Smith" required />
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Email *</label>
                    <input name="email" type="email" value={form.email} onChange={handleChange}
                      placeholder="doctor@clinic.com" required />
                  </div>
                  <div className="form-group">
                    <label>Phone</label>
                    <input name="phone" type="tel" value={form.phone} onChange={handleChange}
                      placeholder="9XXXXXXXXX" />
                  </div>
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Password *</label>
                    <input name="password" type="password" value={form.password} onChange={handleChange}
                      placeholder="Min 6 characters" required minLength={6} />
                  </div>
                  <div className="form-group">
                    <label>Confirm Password *</label>
                    <input name="confirmPassword" type="password" value={form.confirmPassword} onChange={handleChange}
                      placeholder="Repeat password" required />
                  </div>
                </div>
              </div>

              {/* Doctor profile */}
              <div>
                <p style={{ fontSize: '0.8rem', fontWeight: 700, color: '#64748b', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.75rem' }}>Doctor Profile</p>
                <div className="form-group" style={{ marginBottom: '0.75rem' }}>
                  <label>Specialization *</label>
                  <select name="specializationId" value={form.specializationId} onChange={handleChange} required>
                    <option value="">— Select specialization —</option>
                    {specializations.map(s => (
                      <option key={s.specializationId} value={s.specializationId}>{s.name}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group" style={{ marginBottom: '0.75rem' }}>
                  <label>License Number *</label>
                  <input name="licenseNumber" value={form.licenseNumber} onChange={handleChange}
                    placeholder="MH-GM-001" required />
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Years of Experience</label>
                    <input name="yearsOfExperience" type="number" min="0" max="60"
                      value={form.yearsOfExperience} onChange={handleChange} />
                  </div>
                  <div className="form-group">
                    <label>Consultation Fee (₹)</label>
                    <input name="consultationFee" type="number" min="0"
                      value={form.consultationFee} onChange={handleChange} />
                  </div>
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
                  {saving ? 'Creating...' : '✅ Create Doctor Account'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ManageDoctors;
