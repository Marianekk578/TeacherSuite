export const environment = {
  production: true,
  keycloak: {
    url: '${KEYCLOAK_URL}',
    realm: 'teachersuite',
    clientId: 'teachersuite-spa',
  },
};
