import React, { useEffect, useState } from 'react';
import { adminAPI, specializationsAPI } from '../../services/api';
import toast from 'react-hot-toast';

const ManageDoctors = () => {
  const [doctors, setDoctors]               = useState([]);
  const [specializations, setSpecializations] = useState([]);
  const [loading, setLoading]               = useState(true);
  const [showModal, setShowModal]           = useState(false);
  const [deactivating, setDeactivating]     = useState(null);

  const [form, setForm] = useState({
    userId: '', specializationId: '', licenseNumber: '',
    yearsOfExperience: 0, consultationFee: 500,
  });
  const [saving, setSaving] = useState(false);

  const load = () => {
    Promise.all([adminAPI.getDoctors(), specializationsAPI.getAll()])
      .then(([docRes, specRes]) => {
        setDoctors(docRes.data.data || []);
        setSpecializations(specRes.data.data || []);
      })
      .catch(() => toast.error('Failed to load data'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleChange = (e) =>
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

  const handleCreate = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await adminAPI.createDoctor({
        ...form,
        userId:            parseInt(form.userId),
        specializationId:  parseInt(form.specializationId),
        yearsOfExperience: parseInt(form.yearsOfExperience),
        consultationFee:   parseFloat(form.consultationFee),
      });
      toast.success('Doctor created successfully!');
      setShowModal(false);
      setForm({ userId: '', specializationId: '', licenseNumber: '', yearsOfExperience: 0, consultationFee: 500 });
      load();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to create doctor');
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
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          ➕ Add Doctor Profile
        </button>
      </div>

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
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
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

      {/* Create Doctor Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Add Doctor Profile</h3>
              <button className="modal-close" onClick={() => setShowModal(false)}>×</button>
            </div>
            <p className="text-muted mb-2" style={{ fontSize: '0.85rem' }}>
              The user must already be registered with the Doctor role.
            </p>
            <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div className="form-group">
                <label>User ID *</label>
                <input name="userId" type="number" value={form.userId} onChange={handleChange}
                  placeholder="Get from user list" required />
              </div>
              <div className="form-group">
                <label>Specialization *</label>
                <select name="specializationId" value={form.specializationId} onChange={handleChange} required>
                  <option value="">— Select —</option>
                  {specializations.map(s => (
                    <option key={s.specializationId} value={s.specializationId}>{s.name}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>License Number *</label>
                <input name="licenseNumber" value={form.licenseNumber} onChange={handleChange}
                  placeholder="MH-GP-001" required />
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
              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
                  {saving ? 'Creating...' : 'Create Doctor'}
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