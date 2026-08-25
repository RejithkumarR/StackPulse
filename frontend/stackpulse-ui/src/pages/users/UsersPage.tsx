import { useEffect, useState } from 'react';
import { userService, type UserRow } from '../../services/userService';

export default function UsersPage() {
  const [users, setUsers] = useState<UserRow[]>([]);

  useEffect(() => {
    userService.getUsers().then((data) => setUsers(data));
  }, []);

  return (
    <div className="content-panel">
      <div className="section-header">
        <h3>User directory</h3>
        <button className="primary-button small">Create user</button>
      </div>

      <div className="table-card">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.firstName ?? 'User'} {user.lastName ?? ''}</td>
                <td>{user.email}</td>
                <td>{user.role}</td>
                <td>
                  <span className={`status-badge ${user.isActive ? 'active' : 'inactive'}`}>
                    {user.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
