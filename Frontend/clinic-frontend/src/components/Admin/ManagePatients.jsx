import React, { useEffect, useState } from 'react';
import { adminAPI } from '../../services/api';
import toast from 'react-hot-toast';

const ManagePatients = () => {
  const [patients, setPatients]   = useState([]);
  const [loading, setLoading]     = useState(true);
  const [search, setSearch]       = useState('');
  const [selected, setSelected]   = useState(null);
  const [deactivating, setDeactivating] = useState(null);

  const load = (q) => {
    setLoading(true);
    adminAPI.getPatients(q || undefined)
      .then(res => setPatients(res.data.data || []))
      .catch(() => toast.error('Failed to load patients'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    load(search);
  };

  const handleDeactivate = async (userId, name) => {
    if (!window.confirm(`Deactivate patient account for ${name}?`)) return;
    setDeactivating(userId);
    try {
      await adminAPI.deactivateUser(userId);
      toast.success(`${name} deactivated`);
      load(search);
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed');
    } finally {
      setDeactivating(null);
    }
  };

  const viewDetails = (patient) => {
    adminAPI.getPatient(patient.patientId)
      .then(res => setSelected(res.data.data))
      .catch(() => toast.error('Failed to load details'));
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Manage Patients</h1>
          <p className="text-muted">{patients.length} patients found</p>
        </div>
      </div>

      <form onSubmit={handleSearch} className="search-bar">
        <input
          type="text"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search by name or email..."
        />
        <button type="submit" className="btn btn-primary">Search</button>
        {search && (
          <button type="button" className="btn btn-ghost" onClick={() => { setSearch(''); load(); }}>
            Clear
          </button>
        )}
      </form>

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : patients.length === 0 ? (
        <div className="empty-state"><span className="empty-icon">👥</span><h3>No patients found</h3></div>
      ) : (
        <div className="card">
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Gender</th>
                  <th>Blood Group</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {patients.map((p, i) => (
                  <tr key={p.patientId}>
                    <td>{i + 1}</td>
                    <td style={{ fontWeight: 600 }}>{p.fullName}</td>
                    <td>{p.email}</td>
                    <td>{p.phone || '—'}</td>
                    <td>{p.gender || '—'}</td>
                    <td>{p.bloodGroup || '—'}</td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.4rem' }}>
                        <button className="btn btn-ghost btn-sm" onClick={() => viewDetails(p)}>
                          View
                        </button>
                        <button
                          className="btn btn-danger btn-sm"
                          onClick={() => handleDeactivate(p.userId, p.fullName)}
                          disabled={deactivating === p.userId}
                        >
                          {deactivating === p.userId ? '...' : 'Deactivate'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Patient Detail Modal */}
      {selected && (
        <div className="modal-overlay" onClick={() => setSelected(null)}>
          <div className="modal" style={{ maxWidth: '580px' }} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Patient Details</h3>
              <button className="modal-close" onClick={() => setSelected(null)}>×</button>
            </div>
            <div style={{ display: 'grid', gap: '0.75rem', fontSize: '0.9rem' }}>
              {[
                ['Full Name',         selected.fullName],
                ['Email',             selected.email],
                ['Phone',             selected.phone || '—'],
                ['Date of Birth',     selected.dateOfBirth || '—'],
                ['Gender',            selected.gender || '—'],
                ['Blood Group',       selected.bloodGroup || '—'],
                ['Address',           selected.address || '—'],
                ['Emergency Contact', selected.emergencyContact || '—'],
                ['Medical History',   selected.medicalHistory || '—'],
              ].map(([label, value]) => (
                <div key={label} style={{ display: 'flex', gap: '1rem' }}>
                  <span style={{ width: '150px', fontWeight: 600, color: '#475569', flexShrink: 0 }}>{label}</span>
                  <span>{value}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ManagePatients;