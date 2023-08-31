import api from "@/utils/api";
import { defineStore } from "pinia";
export const useGeneralStore = defineStore({
	id: "generico",
	state: () => ({
		items: [],
	}),
	actions: {
		limpiar() {
			this.items = [];
		},
		guardar: async (endPoint, item) => {
			return await api()
				.post(`${window._apiUrl}${endPoint}`, item)
				.then(async (r) => {
					return r.data;
				});
		},
		guardar: async (endPoint, item) => {
			return await api()
				.post(`${window._apiUrl}${endPoint}`, item)
				.then(async (r) => {
					return r.data;
				});
		},
	},
});